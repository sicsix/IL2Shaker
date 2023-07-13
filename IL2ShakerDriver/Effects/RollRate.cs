using IL2ShakerDriver.Samplers;
using IL2TelemetryRelay.Motion;
using NAudio.Wave;

namespace IL2ShakerDriver.Effects;

internal class RollRate : Effect
{
    private const    float         TransitionTime = 0.025f;
    private const    float         MinRollRate    = MathF.PI / 8f;
    private const    float         MaxRollRate    = MathF.PI;
    private const    float         Frequency      = 30;
    private readonly WaveGenerator _waveGenerator = new();

    public RollRate(ISampleProvider source, Audio audio) : base(source, audio)
    {
    }

    protected override void Write(float[] buffer, int offset, int count)
    {
        // Wait 200ms before reducing the output when not at x1 to avoid hiccups due to the sim struggling
        if (Audio.TicksAtAbnormalSpeed > 10)
            _waveGenerator.SetTarget(0, 0, TransitionTime);
        _waveGenerator.Write(buffer, offset, count, Audio.SimClock.Time);
    }

    protected override void OnMotionDataReceived(MotionData motionData)
    {
        float rollRate       = MathF.Max(MathF.Abs(motionData.SpinX) - MinRollRate, 0);
        float rollRateFactor = MathF.Min(rollRate / (MaxRollRate - MinRollRate), 1f);
        float amplitude      = rollRateFactor * Volume.Amplitude;
        _waveGenerator.SetTarget(Frequency, amplitude, TransitionTime);
    }
}