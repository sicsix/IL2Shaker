using IL2ShakerDriver.Samplers;
using IL2TelemetryRelay.State;
using NAudio.Wave;

namespace IL2ShakerDriver.Effects;

internal class GForces : Effect
{
    private const    float         TransitionTime = 0.025f;
    private const    float         MinGForces     = 1.5f;
    private const    float         MaxGForces     = 7f;
    private const    float         Frequency      = 25;
    private readonly WaveGenerator _waveGenerator = new();

    public GForces(ISampleProvider source, Audio audio) : base(source, audio)
    {
        Enabled = true;
    }

    protected override void Write(float[] buffer, int offset, int count)
    {
        // Wait 200ms before reducing the output when not at x1 to avoid hiccups due to the sim struggling
        if (Audio.TicksAtAbnormalSpeed > 10)
            _waveGenerator.SetTarget(0, 0, TransitionTime);
        _waveGenerator.Write(buffer, offset, count, Audio.SimClock.Time);
    }

    protected override void OnStateDataReceived(StateData stateData)
    {
        float gForces = stateData.Acceleration.Length() / 9.8f;
        gForces = MathF.Max(gForces - MinGForces, 0);
        float gForcesFactor = MathF.Min(gForces / (MaxGForces - MinGForces), 1);
        float amplitude     = gForcesFactor * Volume.Amplitude;
        _waveGenerator.SetTarget(Frequency, amplitude, TransitionTime);
    }
}