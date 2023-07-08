namespace IL2ShakerDriver;

internal readonly struct SimTime
{
    public readonly uint Tick;
    public readonly uint SubTick;
    public readonly long AbsoluteTime;

    public SimTime(uint tick, uint subTick)
    {
        Tick         = tick;
        SubTick      = subTick;
        AbsoluteTime = Tick * SimClock.SamplesPerTick + SubTick;
    }

    public SimTime(long absoluteTime)
    {
        Tick         = (uint)(absoluteTime / SimClock.SamplesPerTick);
        SubTick      = (uint)(absoluteTime % SimClock.SamplesPerTick);
        AbsoluteTime = absoluteTime;
    }
}