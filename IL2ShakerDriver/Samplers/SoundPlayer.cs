namespace IL2ShakerDriver.Samplers;

internal class SoundPlayer : ISampleWriter
{
    public bool Complete { get; private set; }

    private readonly SimTime _start;
    private readonly SimTime _end;
    private readonly float[] _sound;
    private readonly float   _amplitude;

    public SoundPlayer(SimTime start, float[] sound, float amplitude)
    {
        Complete   = false;
        _start     = start;
        _end       = new SimTime(_start.AbsoluteTime + sound.Length / 4);
        _sound     = sound;
        _amplitude = amplitude;
    }

    public void Write(float[] buffer, int offset, int count, SimTime simTime)
    {
        int writeStart = Math.Max(0, (int)(_start.AbsoluteTime   - simTime.AbsoluteTime));
        int writeEnd   = Math.Min(count, (int)(_end.AbsoluteTime - simTime.AbsoluteTime));
        int readOffset = (int)(simTime.AbsoluteTime - _start.AbsoluteTime) + writeStart;

        for (int i = writeStart; i < writeEnd; i++)
        {
            float value = buffer[i];
            value     += _sound[readOffset++] * _amplitude;
            buffer[i] =  value;
        }


        if (_end.AbsoluteTime <= simTime.AbsoluteTime + count)
            Complete = true;
    }
}