using System.Numerics;

namespace IL2ShakerDriver.Samplers;

internal class HarmonicsGenerator : ISampleWriter
{
    private readonly Vector4 _harmonics;

    private float _currentFrequency;
    private float _targetFrequency;
    private float _rateOfChangeFrequency;

    private Vector4 _currentAmplitudes;
    private Vector4 _targetAmplitudes;
    private Vector4 _rateOfChangeAmplitudes;

    private int _transitionSamplesRemaining;

    private double _theta;

    public HarmonicsGenerator(int harmonic1, int harmonic2, int harmonic3)
    {
        if (harmonic1 <= 1)
            throw new ArgumentOutOfRangeException(nameof(harmonic1), "Harmonic multiples must be greater than 1");
        if (harmonic2 <= 1)
            throw new ArgumentOutOfRangeException(nameof(harmonic2), "Harmonic multiples must be greater than 1");
        if (harmonic3 <= 1)
            throw new ArgumentOutOfRangeException(nameof(harmonic3), "Harmonic multiples must be greater than 1");

        _harmonics = new Vector4(1f, harmonic1, harmonic2, harmonic3);
    }

    public void SetTarget(float fundamentalFreq, Vector4 amplitudes, float transitionTime)
    {
        _targetFrequency            = fundamentalFreq;
        _targetAmplitudes           = amplitudes;
        _transitionSamplesRemaining = (int)(transitionTime * SimClock.SampleRate);
        _rateOfChangeFrequency      = (_targetFrequency  - _currentFrequency)  / _transitionSamplesRemaining;
        _rateOfChangeAmplitudes     = (_targetAmplitudes - _currentAmplitudes) / _transitionSamplesRemaining;
    }

    public void Write(float[] buffer, int offset, int count, SimTime simTime)
    {
        if ((_targetAmplitudes == Vector4.Zero && _currentAmplitudes == Vector4.Zero)
         || (_targetFrequency  == 0            && _currentFrequency  == 0))
            return;

        double thetaIncrement = _currentFrequency * (Math.PI * 2) / SimClock.SampleRate;

        for (int i = 0; i < count; i++)
        {
            float value = buffer[offset + i];

            var thetas = _harmonics * (float)_theta;

            value += _currentAmplitudes[0] * MathF.Sin(thetas[0]);
            value += _currentAmplitudes[1] * MathF.Sin(thetas[1]);
            value += _currentAmplitudes[2] * MathF.Sin(thetas[2]);
            value += _currentAmplitudes[3] * MathF.Sin(thetas[3]);

            buffer[offset + i] = value;

            _theta += thetaIncrement;
            _theta %= Math.PI * 2;

            if (_transitionSamplesRemaining == -1)
                continue;

            if (_transitionSamplesRemaining == 0)
            {
                _currentFrequency  = _targetFrequency;
                _currentAmplitudes = _targetAmplitudes;
            }
            else
            {
                _currentFrequency  += _rateOfChangeFrequency;
                _currentAmplitudes += _rateOfChangeAmplitudes;
            }

            thetaIncrement = _currentFrequency * (Math.PI * 2) / SimClock.SampleRate;
            _transitionSamplesRemaining--;
        }
    }
}