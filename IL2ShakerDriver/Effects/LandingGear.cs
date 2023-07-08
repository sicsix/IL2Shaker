using System.Numerics;
using IL2ShakerDriver.Samplers;
using IL2TelemetryRelay;
using IL2TelemetryRelay.Events;
using IL2TelemetryRelay.State;
using NAudio.Wave;

namespace IL2ShakerDriver.Effects;

internal class LandingGear : Effect
{
    private readonly List<ImpulseGenerator>   _impulseGenerators   = new();
    private readonly List<HarmonicsGenerator> _harmonicsGenerators = new();
    private readonly float[]                  _previousPosition    = new float[4];
    private readonly bool[]                   _actuating           = new bool[4];
    private readonly Vector4                  _harmonics           = new(1, 2, 3, 4);

    private const float ActuatingBaseFreq  = 50;
    private const float BeginActuatingFreq = 80;
    private const float RetractedFreq      = 70;
    private const float ExtendedFreq       = 35;

    private Vector4 _distance;
    private Vector4 _actuatingAmplitudes;
    private Volume  _beginActuatingVolume;
    private Volume  _retractedVolume;
    private Volume  _extendedVolume;

    public LandingGear(ISampleProvider source, Audio audio) : base(source, audio)
    {
        _harmonicsGenerators.Add(new HarmonicsGenerator(2, 3, 4));
        _harmonicsGenerators.Add(new HarmonicsGenerator(2, 3, 4));
        _harmonicsGenerators.Add(new HarmonicsGenerator(2, 3, 4));
        _harmonicsGenerators.Add(new HarmonicsGenerator(2, 3, 4));
    }

    protected override void OnSettingsUpdated()
    {
        _actuatingAmplitudes  = new Vector4(GetAmplitude(-12), GetAmplitude(-18), 0, 0);
        _retractedVolume      = GetVolume(0);
        _extendedVolume       = GetVolume(-5);
        _beginActuatingVolume = GetVolume(-6);
    }

    protected override void Write(float[] buffer, int offset, int count)
    {
        for (int i = 0; i < _impulseGenerators.Count; i++)
        {
            if (Audio.SimSpeed != SimSpeed.x1 || _impulseGenerators[i].Complete)
            {
                _impulseGenerators.RemoveAt(i--);
                continue;
            }

            _impulseGenerators[i].Write(buffer, offset, count, Audio.SimClock.Time);
        }

        for (int i = 0; i < _harmonicsGenerators.Count; i++)
        {
            if (Audio.SimSpeed != SimSpeed.x1)
                _harmonicsGenerators[i].SetTarget(0, Vector4.Zero, 0.1f);
            _harmonicsGenerators[i].Write(buffer, offset, count, Audio.SimClock.Time);
        }
    }

    protected override void OnEventDataReceived(Event eventData)
    {
        if (eventData is not WheelDataEvent wheelDataEvent)
            return;

        _distance[wheelDataEvent.Index] = wheelDataEvent.Offset.Length();
    }

    protected override void OnStateDataReceived(StateData stateData)
    {
        for (int i = 0; i < 4; i++)
        {
            float position = stateData.LandingGearPosition[i];
            // It's only actuating if it has slightly moved since the previous packet
            // This gets rid of false actuations/extensions on fixed gear aircraft etc.
            float diff      = Math.Abs(position - _previousPosition[i]);
            bool  actuating = diff is > 0.01f and < 0.25f;

            bool wasActuating = _actuating[i];

            if (stateData.Paused && actuating)
            {
                actuating = false;
                // Stop the generators
                _harmonicsGenerators[i].SetTarget(ActuatingBaseFreq, Vector4.Zero, 0.5f);
            }
            else if (!actuating && wasActuating)
            {
                // Stop the generator
                _harmonicsGenerators[i].SetTarget(ActuatingBaseFreq, Vector4.Zero, 0.5f);

                if (position == 0)
                {
                    Logging.At(this).Debug("Landing gear {Index} fully retracted", i);
                    // Play fully retracted sound
                    float amp = Attenuate(RetractedFreq, _retractedVolume.Amplitude, _distance[i]);
                    _impulseGenerators.Add(new ImpulseGenerator(RetractedFreq, amp, 7, 5));
                    _impulseGenerators.Add(new ImpulseGenerator(RetractedFreq, amp, 7, 5, 0.2f));
                }
                else if (position == 1)
                {
                    Logging.At(this).Debug("Landing gear {Index} fully extended", i);
                    // Play fully extended sound
                    float amp0 = Attenuate(ExtendedFreq, _extendedVolume.Amplitude, _distance[i]);
                    float amp1 = Attenuate(ExtendedFreq * 1.5f, _extendedVolume.Amplitude, _distance[i]);
                    _impulseGenerators.Add(new ImpulseGenerator(ExtendedFreq, amp0, 5, 3));
                    _impulseGenerators.Add(new ImpulseGenerator(ExtendedFreq * 1.5f, amp1, 7, 3, 0.15f));
                }
            }
            else if (actuating && !wasActuating)
            {
                Logging.At(this).Debug("Landing gear {Index} actuating", i);
                if (_previousPosition[i] is 0 or 1)
                {
                    // Play start sound
                    float amp = Attenuate(BeginActuatingFreq, _beginActuatingVolume.Amplitude, _distance[i]);
                    _impulseGenerators.Add(new ImpulseGenerator(BeginActuatingFreq, amp, 8, 6));
                    _impulseGenerators.Add(new ImpulseGenerator(BeginActuatingFreq, amp, 8, 6, 0.1f));
                }

                // Start the generator
                var frequencies = _harmonics * ActuatingBaseFreq;
                var amplitudes  = _actuatingAmplitudes;
                amplitudes[0] = Attenuate(frequencies[0], amplitudes[0], _distance[i]);
                amplitudes[1] = Attenuate(frequencies[1], amplitudes[1], _distance[i]);
                amplitudes[2] = Attenuate(frequencies[2], amplitudes[2], _distance[i]);
                amplitudes[3] = Attenuate(frequencies[3], amplitudes[3], _distance[i]);
                _harmonicsGenerators[i].SetTarget(ActuatingBaseFreq, _actuatingAmplitudes, 0.25f);
            }

            _actuating[i]        = actuating;
            _previousPosition[i] = position;
        }
    }
}