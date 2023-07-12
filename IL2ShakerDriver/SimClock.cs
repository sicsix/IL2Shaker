using System.Numerics;

namespace IL2ShakerDriver;

internal class SimClock
{
    public const int  SampleRate         = 44100;
    public const uint UpdateRate         = 50;
    public const uint SamplesPerTick     = SampleRate     / UpdateRate;
    public const uint SamplesPerHalfTick = SamplesPerTick / 2;
    public const uint MsPerTick          = 1000           / UpdateRate;

    private static readonly float[] SimSpeedRatios = { 0, 1 / 32f, 1 / 16f, 1 / 8f, 1 / 4f, 1 / 2f, 1f, 2f, 4f, 8f };

    private static readonly float[] SimSpeedMsTick =
    {
        float.PositiveInfinity,
        32f * MsPerTick,
        16f * MsPerTick,
        8f  * MsPerTick,
        4f  * MsPerTick,
        2f  * MsPerTick,
        MsPerTick,
        MsPerTick,
        MsPerTick,
        MsPerTick
    };

    public SimTime Time { get; private set; }

    public SimSpeed UpdateTick(uint tick, int latency, bool paused)
    {
        var simSpeed    = GetSimSpeed(tick, paused);
        var currentTime = GetLatencyOffsetSimTime(tick, latency);

        // If the simspeed isn't 1 just keep setting the tick to the current game tick, we won't be playing audio
        if (simSpeed != SimSpeed.x1)
            Time = currentTime;
        else if (Time.AbsoluteTime < currentTime.AbsoluteTime - SamplesPerTick)
        {
            // We are too far behind, need to reset
            Logging.At(this)
               .Warning("Correcting out of sync time, delayed - {Tick}:{SubTick} => {NewTick}:{NewSubTick}", Time.Tick,
                        Time.SubTick, currentTime.Tick, currentTime.SubTick);
            Time = currentTime;
        }
        else if (Time.Tick >= tick)
        {
            // We are too far in front, need to reset
            Logging.At(this)
               .Warning("Correcting out of sync time, advanced - {Tick}:{SubTick} => {NewTick}:{NewSubTick}", Time.Tick,
                        Time.SubTick, currentTime.Tick, currentTime.SubTick);
            Time = currentTime;
        }

        return simSpeed;
    }

    private static SimTime GetLatencyOffsetSimTime(uint tick, int latency)
    {
        return new SimTime((long)tick * SamplesPerTick - (long)(latency * 0.001f * SampleRate) - SamplesPerTick);
    }

    public void Increment(int samples)
    {
        Time = new SimTime(Time.AbsoluteTime + samples);
    }


    private Vector2 _tickDiffs;
    private int     _tickIndex;
    private float   _prevTick;

    private Vector3 _msDiffs;
    private int     _msIndex;

    private int _count;

    private DateTime _prevTime;

    private SimSpeed _prevSpeed;

    private SimSpeed GetSimSpeed(uint tick, bool paused)
    {
        if (paused)
        {
            _prevSpeed = SimSpeed.Paused;
            _count     = 0;
            return SimSpeed.Paused;
        }

        var   time   = DateTime.UtcNow;
        float msDiff = (float)(time - _prevTime).TotalMilliseconds;
        _prevTime = time;

        // Discard if the time is large or too small - probably due to unpausing
        if (msDiff > SimSpeedMsTick[(int)SimSpeed.x1_32] * 1.2f || msDiff < SimSpeedMsTick[(int)SimSpeed.x1] * 0.8f)
            return _prevSpeed;

        // Calculate difference in time thas this and previous tick were received
        _msDiffs[_msIndex] = msDiff;
        _msIndex           = (_msIndex + 1) % 3;

        // Calculate difference between this tick and previous
        float tickDiff = tick - _prevTick;
        _prevTick              = tick;
        _tickDiffs[_tickIndex] = tickDiff;
        _tickIndex             = (_tickIndex + 1) % 2;

        // Don't try and guess speed unless we have at least 3 samples
        _count++;
        if (_count < 3)
            return _prevSpeed;

        // The diff for speeds > 1 is the difference in tick number
        // Not using history for this at the moment
        if (tickDiff > 1 && _tickDiffs[0] == _tickDiffs[1])
        {
            int offset = BitOperations.TrailingZeroCount((int)tickDiff);
            return GetSimSpeedBasedOnPrev((SimSpeed)(offset + (int)SimSpeed.x1));
        }

        // The diff for speeds <= 1 is the time between ticks
        // Gets the average time for the previous 3 ticks and works out which speed is closest
        float avgMs     = (_msDiffs.X + _msDiffs.Y + _msDiffs.Z) / 3f;
        float minMsDiff = float.MaxValue;
        int   closest   = 1;
        for (int i = (int)SimSpeed.x1_32; i <= (int)SimSpeed.x1; i++)
        {
            float diff = MathF.Abs(SimSpeedMsTick[i] - avgMs);
            if (diff > minMsDiff)
                continue;
            minMsDiff = diff;
            closest   = i;
        }

        return GetSimSpeedBasedOnPrev((SimSpeed)closest);
    }

    private SimSpeed GetSimSpeedBasedOnPrev(SimSpeed simSpeed)
    {
        // Returns the latest speed only if it matches the previous speed
        if (simSpeed == _prevSpeed)
            return simSpeed;

        var outSpeed = _prevSpeed;
        _prevSpeed = simSpeed;
        return outSpeed;
    }
}