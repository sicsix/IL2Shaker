using System.Numerics;
using IL2ShakerDriver.Samplers;
using IL2TelemetryRelay;
using IL2TelemetryRelay.Events;
using IL2TelemetryRelay.State;
using NAudio.Wave;

namespace IL2ShakerDriver.Effects;

internal class Bumps : Effect
{
    private readonly List<ImpulseGenerator> _impulseGenerators = new();

    private readonly bool[] _rising      = new bool[4];
    private readonly int[]  _ticksRising = new int[4];

    private Vector4 _prevPressures;
    private Vector4 _startPressure;
    private Vector4 _distance;

    private const float MaxRateOfChange = 0.4f;
    private const float MinFreq         = 25;
    private const float MaxFreq         = 40;

    public Bumps(ISampleProvider source, Audio audio) : base(source, audio)
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
        if (eventData is WheelDataEvent wheelDataEvent)
            _distance[wheelDataEvent.Index] = wheelDataEvent.Offset.Length();
    }

    protected override void OnStateDataReceived(StateData stateData)
    {
        var prev = _prevPressures;
        var diff = stateData.LandingGearPressure - prev;
        _prevPressures = stateData.LandingGearPressure;

        // Skip playing bumps for the first 5 seconds while aircraft is spawning in
        if (stateData.Tick < 250)
            return;

        for (int i = 0; i < 4; i++)
        {
            bool wasRising = _rising[i];
            bool rising    = diff[i] > 0;
            _rising[i] = rising;

            if (!rising && wasRising)
            {
                float pressureChange = prev[i] - _startPressure[i];
                float rateOfChange   = pressureChange / _ticksRising[i];
                // Logging.At(this)
                //    .Debug("Tick = {Tick}, Wheel = {i}, PressureChange = {Diff}, OverTicks = {Ticks}, RateOfChange = {Rate}",
                //           stateData.Tick, i, pressureChange, _ticksRising[i], rateOfChange);

                rateOfChange = Math.Min(MathF.Sqrt(rateOfChange), MaxRateOfChange);
                float ratio = rateOfChange / MaxRateOfChange;
                float amp1  = ratio        * Volume.Amplitude;

                float freq1 = MinFreq + (1 - ratio) * (MaxFreq - MinFreq);

                amp1 = Attenuate(freq1, amp1, _distance[i]);

                _impulseGenerators.Add(new ImpulseGenerator(freq1, amp1, 5, 3));

                _ticksRising[i]   = 0;
                _startPressure[i] = 0;
            }
            else if (rising && !wasRising)
            {
                _startPressure[i] = prev[i];
                _ticksRising[i]++;
            }
            else if (rising)
                _ticksRising[i]++;
        }
    }
}