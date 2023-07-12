using IL2ShakerDriver.Samplers;
using IL2TelemetryRelay;
using IL2TelemetryRelay.Events;
using NAudio.Wave;

namespace IL2ShakerDriver.Effects;

internal class HitsReceived : Effect
{
    private readonly List<ImpulseGenerator> _impulseGenerators = new();

    private const float MinSqrKineticEnergy = 0f;
    private const float MaxSqrKineticEnergy = 150f;
    private const float AmpMin              = 0f;
    private const float AmpMax              = 1f;
    private const float FreqMin             = 18f;
    private const float FreqMax             = 53.33333f;
    private const float SecondFreqMult      = 1.5f;
    private const float SecondAmpMult       = 2.0f;

    public HitsReceived(ISampleProvider source, Audio audio) : base(source, audio)
    {
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
        if (eventData is HitEvent hitEvent)
        {
            Logging.At(this).Debug("Tick = {tick}, Offset = {offset}, Energy = {energy}", hitEvent.Tick,
                                   hitEvent.Offset, hitEvent.Force.Length());

            float keRatio = (MathF.Sqrt(hitEvent.Force.Length()) - MinSqrKineticEnergy)
                          / (MaxSqrKineticEnergy                 - MinSqrKineticEnergy);
            keRatio = Math.Clamp(keRatio, 0f, 1f);
            float freq1 = FreqMin + (1 - keRatio) * (FreqMax - FreqMin);
            float amp1  = AmpMin  + keRatio       * (AmpMax  - AmpMin);
            amp1 *= Volume.Amplitude;
            float freq2 = freq1 * SecondFreqMult;
            float amp2  = amp1  * SecondAmpMult;

            float dist = hitEvent.Offset.Length();
            amp1 = Attenuate(freq1, amp1, dist);
            amp2 = Attenuate(freq2, amp2, dist);

            _impulseGenerators.Add(new ImpulseGenerator(freq1, amp1, 3, 2));
            _impulseGenerators.Add(new ImpulseGenerator(freq2, amp2, 6, 4));
        }
    }
}