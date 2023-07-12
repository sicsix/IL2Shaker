namespace IL2ShakerDriver.Samplers;

internal class TimedImpulseGenerator : ISampleWriter
{
    public bool Complete { get; private set; }

    private readonly SimTime _start;
    private readonly float   _amplitude;
    private readonly int     _releaseSamples;
    private readonly double  _thetaIncrement;

    private int _sampleIndex;
    private int _samplesRemaining;

    public TimedImpulseGenerator(SimTime start,
                                 float   frequency,
                                 float   amplitude,
                                 int     cycles,
                                 int     releaseStartCycle)
    {
        _start     = start;
        _amplitude = amplitude;
        float samplesPerCycle = SimClock.SampleRate            / frequency;
        _releaseSamples   = (int)((cycles - releaseStartCycle) * samplesPerCycle);
        _thetaIncrement   = frequency * (Math.PI * 2) / SimClock.SampleRate;
        _sampleIndex      = 0;
        _samplesRemaining = (int)(cycles * samplesPerCycle);
    }

    public void Write(float[] buffer, int offset, int count, SimTime simTime)
    {
        int timeOffset = (int)(_start.AbsoluteTime - simTime.AbsoluteTime);
        int writeStart = Math.Max(0, timeOffset);
        int writeEnd   = Math.Min(count, timeOffset + _samplesRemaining);

        for (int i = writeStart; i < writeEnd; i++)
        {
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