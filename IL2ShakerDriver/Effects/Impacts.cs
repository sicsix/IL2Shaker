using System.Numerics;
using IL2ShakerDriver.Samplers;
using IL2TelemetryRelay.State;
using NAudio.Wave;

namespace IL2ShakerDriver.Effects;

internal class Impacts : Effect
{
    private Vector3 _previousAcceleration;

    private readonly AdditiveRumbleGenerator _additiveRumbleGenerator =
        new(Frequency, MaxIntensity, IntensityMultiplier, MaxTime);

    private const float Frequency           = 20;
    private const float MinIntensity        = 15;
    private const float MaxIntensity        = 1000;
    private const float IntensityMultiplier = 1000;
    private const float MaxTime             = 2;

    public Impacts(ISampleProvider source, Audio audio) : base(source, audio)
    {
    }

    protected override void OnSettingsUpdated()
    {
        _additiveRumbleGenerator.SetVolume(Volume);
    }

    protected override void OnStateDataReceived(StateData stateData)
    {
        var prev = _previousAcceleration;
        _previousAcceleration = stateData.Acceleration;
        
        // Skip playing impacts for the first 5 seconds while aircraft is spawning in
        if (stateData.Tick < 250)
            return;

        if (stateData.Acceleration == Vector3.Zero || prev == Vector3.Zero)
            return;

        var   diff    = stateData.Acceleration - prev;
        float impulse = diff.Length();
        if (impulse > MinIntensity)
        {
            Logging.At(this).Debug("Tick: {Tick}    Impulse: {Imp}", stateData.Tick, impulse);
            _additiveRumbleGenerator.AddToIntensity(impulse);
        }
    }

    protected override void Write(float[] buffer, int offset, int count)
    {
        _additiveRumbleGenerator.Write(buffer, offset, count, Audio.SimClock.Time);
    }
}