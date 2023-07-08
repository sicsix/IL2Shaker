namespace IL2ShakerDriver.Samplers;

internal class WaveGenerator : ISampleWriter
{
    private float _currentFrequency;
    private float _targetFrequency;
    private float _rateOfChangeFrequency;

    private float _currentAmplitude;
    private float _targetAmplitude;
    private float _rateOfChangeAmplitude;

    private int _transitionSamplesRemaining;

    private double _theta;

    public void SetTarget(float freq, float amplitude, float transitionTime)
    {
        _targetFrequency            = freq;
        _targetAmplitude            = amplitude;
        _transitionSamplesRemaining = (int)(transitionTime * SimClock.SampleRate);
        _rateOfChangeFrequency      = (_targetFrequency - _currentFrequency) / _transitionSamplesRemaining;
        _rateOfChangeAmplitude      = (_targetAmplitude - _currentAmplitude) / _transitionSamplesRemaining;
    }

    public void Write(float[] buffer, int offset, int count, SimTime simTime)
    {
        if ((_currentAmplitude == 0 && _targetAmplitude == 0) || (_currentFrequency == 0 && _targetFrequency == 0))
            return;

        double thetaIncrement = _currentFrequency * (Math.PI * 2) / SimClock.SampleRate;

        for (int i = 0; i < count; i++)
        {
            float value = buffer[offset + i];

            value += _currentAmplitude * MathF.Sin((float)_theta);

            buffer[offset + i] = value;

            _theta += thetaIncrement;
            _theta %= Math.PI * 2;

            if (_transitionSamplesRemaining == -1)
                continue;

            if (_transitionSamplesRemaining == 0)
            {
                _currentFrequency = _targetFrequency;
                _currentAmplitude = _targetAmplitude;
            }
            else
            {
                _currentFrequency += _rateOfChangeFrequency;
                _currentAmplitude += _rateOfChangeAmplitude;
            }

            thetaIncrement = _currentFrequency * (Math.PI * 2) / SimClock.SampleRate;
            _transitionSamplesRemaining--;
        }
    }
}