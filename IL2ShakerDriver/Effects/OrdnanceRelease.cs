using IL2ShakerDriver.Samplers;
using IL2TelemetryRelay;
using IL2TelemetryRelay.Events;
using NAudio.Wave;

namespace IL2ShakerDriver.Effects;

internal class OrdnanceRelease : Effect
{
    private readonly List<ImpulseGenerator> _impulseGenerators = new();

    private const float MaxMass = 2500f;
    private const float MinAmp  = 0.2f;

    public OrdnanceRelease(ISampleProvider source, Audio audio) : base(source, audio)
    {
        // TODO STUB need to finish this.
    }

    protected override void Write(float[] buffer, int offset, int count)
    {
        for (int i = 0; i < _impulseGenerators.Count; i++)
        {
            if (_impulseGenerators[i].Complete)
            {
                _impulseGenerators.RemoveAt(i--);
                continue;
            }

            _impulseGenerators[i].Write(buffer, offset, count, Audio.SimClock.Time);
        }
    }

    protected override void OnEventDataReceived(Event eventData)
    {
        if (eventData is BombReleaseEvent bombReleaseEvent)
        {
            float freq = 0;
            float amp  = 0;
            float dist = bombReleaseEvent.Offset.Length();

            Logging.At(this).Debug("Tick = {Tick}, Bomb Mass = {Mass}", bombReleaseEvent.Tick, bombReleaseEvent.Mass);

            amp = Attenuate(freq, amp, dist);
        }
        else if (eventData is RocketLaunchEvent rocketLaunchEvent)
        {
            float freq = 0;
            float amp  = 0;
            float dist = rocketLaunchEvent.Offset.Length();

            Logging.At(this).Debug("Tick = {Tick}, Rocket Mass = {Mass}", rocketLaunchEvent.Tick,
                                   rocketLaunchEvent.Mass);

            amp = Attenuate(freq, amp, dist);
        }
    }
}