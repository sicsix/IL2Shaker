using IL2ShakerDriver.DB;
using IL2ShakerDriver.Samplers;
using IL2TelemetryRelay;
using IL2TelemetryRelay.Events;
using NAudio.Wave;
using Serilog;
using Serilog.Events;

namespace IL2ShakerDriver.Effects;

internal class GunFire : Effect
{
    private readonly List<TimedImpulseGenerator> _impulseGenerators = new();
    private readonly List<GunState>              _gunStates         = new();
    private readonly List<Gun>                   _guns              = new();
    private readonly List<float>                 _distances         = new();
    private readonly GunRPMCalculator            _gunRPMCalculator;

    private string _aircraftName = string.Empty;

    private Volume _primaryFreqMinVolume;
    private Volume _primaryFreqMaxVolume;
    private Volume _secondFreqVolumeDiff;

    private const float MinSqrKineticEnergy = 1.86011f;
    private const float MaxSqrKineticEnergy = 50.149f;
    private const float FreqMin             = 18f;
    private const float FreqMax             = 53.33333f;
    private const float SecondFreqMult      = 1.5f;


    private class GunState
    {
        public bool    Firing;
        public SimTime LastFired;
        public SimTime LastPlayed;
    }

    public GunFire(ISampleProvider source, Audio audio) : base(source, audio)
    {
        _gunRPMCalculator = new GunRPMCalculator(_guns);
    }

    protected override void OnSettingsUpdated()
    {
        _primaryFreqMinVolume = GetVolume(-15);
        _primaryFreqMaxVolume = GetVolume(-6);
        _secondFreqVolumeDiff = new Volume(6);
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
    }

    protected override void OnUpdate(SimTime simTime)
    {
        for (int i = 0; i < _gunStates.Count; i++)
        {
            var state = _gunStates[i];

            if (Log.IsEnabled(LogEventLevel.Debug))
                _gunRPMCalculator.CalculateIfStopped(simTime, i);

            if (!state.Firing)
                continue;

            var gun = _guns[i];

            float expectedUpdateRate  = 50f / (gun.RPM / 60f);
            int   maxDiffBetweenShots = (int)Math.Ceiling(expectedUpdateRate);
            if ((int)(simTime.Tick - state.LastFired.Tick) - 1 >= maxDiffBetweenShots)
            {
                state.Firing     = false;
                state.LastPlayed = default;
                continue;
            }

            if (state.LastPlayed.Tick == 0)
            {
                var playAt = new SimTime(state.LastFired.AbsoluteTime + SimClock.SamplesPerTick);
                AddImpulses(playAt, state, gun.SqrKineticEnergy, _distances[i]);
            }
            else if (simTime.AbsoluteTime - state.LastPlayed.AbsoluteTime - SimClock.SamplesPerTick
                   > gun.SamplesBetweenShots)
            {
                var playAt = new SimTime(state.LastPlayed.AbsoluteTime + gun.SamplesBetweenShots);
                AddImpulses(playAt, state, gun.SqrKineticEnergy, _distances[i]);
            }
        }
    }

    private void AddImpulses(SimTime playAt, GunState state, float sqrKe, float distance)
    {
        float keRatio = (sqrKe - MinSqrKineticEnergy) / (MaxSqrKineticEnergy - MinSqrKineticEnergy);
        float freq1   = FreqMin + (1 - keRatio) * (FreqMax - FreqMin);
        float amp1 = _primaryFreqMinVolume.Amplitude
                   + keRatio * (_primaryFreqMaxVolume.Amplitude - _primaryFreqMinVolume.Amplitude);
        float freq2 = freq1 * SecondFreqMult;
        float amp2  = amp1  * _secondFreqVolumeDiff.Amplitude;

        amp1 = Attenuate(freq1, amp1, distance);
        amp2 = Attenuate(freq2, amp2, distance);

        _impulseGenerators.Add(new TimedImpulseGenerator(playAt, freq1, amp1, 3, 2));
        _impulseGenerators.Add(new TimedImpulseGenerator(playAt, freq2, amp2, 6, 4));
        state.LastPlayed = playAt;
    }

    protected override void OnEventDataReceived(Event eventData)
    {
        switch (eventData)
        {
            case AircraftNameEvent aircraftName:
                if (aircraftName.Name != _aircraftName)
                {
                    Logging.At(this).Debug("Aircraft: {Name}", aircraftName.Name);
                    _guns.Clear();
                    _gunStates.Clear();
                    _distances.Clear();
                }

                _aircraftName = aircraftName.Name;
                _gunRPMCalculator.SetAircraftName(_aircraftName);
                break;
            case GunDataEvent gunData:
                while (_guns.Count < gunData.Index + 1)
                {
                    _guns.Add(default!);
                    _gunStates.Add(new GunState());
                    _distances.Add(default);
                }

                var gun = Audio.Database.GetGun(new GunID(_aircraftName, gunData.Index, gunData.Velocity,
                                                          gunData.Mass));
                if (gun.Name != "Unknown" && _guns[gunData.Index] != gun)
                    Logging.At(this).Debug("Gun {Idx}: {Gun}", gunData.Index, gun);
                _guns[gunData.Index]      = gun;
                _distances[gunData.Index] = gunData.Offset.Length();
                break;
            case GunFiredEvent gunFired:

                if (_gunStates.Count == 0)
                    break;
                _gunStates[gunFired.Index].Firing = true;
                if (Log.IsEnabled(LogEventLevel.Debug))
                    _gunRPMCalculator.UpdateFireRate(gunFired.Tick, gunFired.Index);
                _gunStates[gunFired.Index].LastFired = new SimTime(gunFired.Tick, 0);

                break;
        }
    }
}