namespace IL2ShakerDriver.Samplers;

internal class AdditiveRumbleGenerator : ISampleWriter
{
    private readonly double _thetaIncrement;
    private readonly float  _maxIntensity;
    private readonly float  _intensityMultiplier;
    private readonly float  _maxTime;

    private Volume _volume;
    private int    _samplesRemaining;
    private int    _sampleIndex;

    public AdditiveRumbleGenerator(float frequency, float maxIntensity, float intensityMultiplier, float maxTime)
    {
        _thetaIncrement      = frequency * (Math.PI * 2) / SimClock.SampleRate;
        _maxIntensity        = maxIntensity;
        _intensityMultiplier = intensityMultiplier;
        _maxTime             = maxTime;
    }

    public void SetVolume(Volume volume)
    {
        // _volume = volume;
        _volume = volume;
    }

    public void AddToIntensity(float value)
    {
        float intensity = Math.Min(value, _maxIntensity);
        _samplesRemaining = (int)Math.Min(_samplesRemaining + intensity * _intensityMultiplier,
                                          SimClock.SampleRate * _maxTime);
    }

    public void Write(float[] buffer, int offset, int count, SimTime simTime)
    {
        for (int i = 0; i < count; i++)
        {
            if (_samplesRemaining == 0)
            {
                _sampleIndex = 0;
                break;
            }

            float value = buffer[i];

            float intensity = _samplesRemaining / (SimClock.SampleRate * _maxTime);
            float amplitude = intensity         * _volume.Amplitude;
            float theta     = (float)(_thetaIncrement * _sampleIndex);

            value += amplitude * MathF.Sin(theta);

            buffer[i] = value;

            _samplesRemaining--;
            _sampleIndex++;
        }
    }
}