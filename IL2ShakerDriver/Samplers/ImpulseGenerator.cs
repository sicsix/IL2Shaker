namespace IL2ShakerDriver.Samplers;

internal class ImpulseGenerator : ISampleWriter
{
    public bool Complete { get; private set; }

    private readonly float  _amplitude;
    private readonly int    _releaseSamples;
    private readonly double _thetaIncrement;

    private int _delay;
    private int _sampleIndex;
    private int _samplesRemaining;

    public ImpulseGenerator(float frequency,
                            float amplitude,
                            int   cycles,
                            int   releaseStartCycle,
                            float delay = 0)
    {
        _amplitude = amplitude;
        float samplesPerCycle = SimClock.SampleRate            / frequency;
        _releaseSamples   = (int)((cycles - releaseStartCycle) * samplesPerCycle);
        _thetaIncrement   = frequency * (Math.PI * 2) / SimClock.SampleRate;
        _sampleIndex      = 0;
        _delay            = (int)(SimClock.SampleRate * delay);
        _samplesRemaining = (int)(cycles * samplesPerCycle) + _delay;
    }

    public void Write(float[] buffer, int offset, int count, SimTime simTime)
    {
        int writeEnd = Math.Min(count, _samplesRemaining);

        for (int i = 0; i < writeEnd; i++)
        {
            if (_delay > 0)
            {
                _delay--;
                _samplesRemaining--;
                continue;
            }

            float value     = buffer[i];
            float amplitude = Math.Min(_samplesRemaining / (float)_releaseSamples, 1.0f) * _amplitude;
            float theta     = (float)(_thetaIncrement * _sampleIndex);
            value += amplitude * MathF.Sin(theta);

            buffer[i] = value;
            _sampleIndex++;
            _samplesRemaining--;
        }

        if (_samplesRemaining <= 0)
            Complete = true;
    }
}