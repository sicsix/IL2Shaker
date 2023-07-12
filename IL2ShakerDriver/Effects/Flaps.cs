using System.Numerics;
using IL2ShakerDriver.Samplers;
using IL2TelemetryRelay.State;
using NAudio.Wave;

namespace IL2ShakerDriver.Effects;

internal class Flaps : Effect
{
    private readonly HarmonicsGenerator _harmonicsGenerator = new(2, 3, 4);
    private          float              _previousPosition;
    private          bool               _actuating;

    private const float ActuatingBaseFreq = 28;

    private Vector4 _actuatingAmplitudes;

    public Flaps(ISampleProvider source, Audio audio) : base(source, audio)
    {
    }

    protected override void OnSettingsUpdated()
    {
        _actuatingAmplitudes = new Vector4(0, GetAmplitude(-18), GetAmplitude(-9), 0);
    }

    protected override void Write(float[] buffer, int offset, int count)
    {
        // Wait 200ms before reducing the output when not at x1 to avoid hiccups due to the sim struggling
        if (Audio.TicksAtAbnormalSpeed > 10)
            _harmonicsGenerator.SetTarget(0, Vector4.Zero, 0.1f);
        _harmonicsGenerator.Write(buffer, offset, count, Audio.SimClock.Time);
    }

    protected override void OnStateDataReceived(StateData stateData)
    {
        float position = stateData.FlapsPosition;
        // It's only actuating if it has slightly moved since the previous packet
        float diff      = Math.Abs(position - _previousPosition);
        bool  actuating = diff is not 0 and < 0.25f;

        bool wasActuating = _actuating;

        if (stateData.Paused && actuating)
        {
            actuating = false;
            // Stop the generator
            _harmonicsGenerator.SetTarget(ActuatingBaseFreq, Vector4.Zero, 0.25f);
        }
        else if (!actuating && wasActuating)
        {
            // Stop the generator
            _harmonicsGenerator.SetTarget(ActuatingBaseFreq, Vector4.Zero, 0.25f);
        }
        else if (actuating && !wasActuating)
        {
            // Start the generator
            _harmonicsGenerator.SetTarget(ActuatingBaseFreq, _actuatingAmplitudes, 0.25f);
        }

        _actuating        = actuating;
        _previousPosition = position;
    }
}