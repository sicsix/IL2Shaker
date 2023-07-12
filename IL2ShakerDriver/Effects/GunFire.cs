using IL2ShakerDriver.DB;
using IL2ShakerDriver.Samplers;
using IL2TelemetryRelay;
using IL2TelemetryRelay.Events;
using IL2TelemetryRelay.State;
using NAudio.Wave;
using Serilog;
using Serilog.Events;

namespace IL2ShakerDriver.Effects;

internal class GunFire : Effect
{
    private readonly List<TimedImpulseGenerator> _impulseGenerators = new();
    private readonly List<GunState>              _gunStates         = new();
    private readonly List<Gun>                   _guns              = new();
    private readonly List<bool>                  _aggregated        = new();
    private readonly List<float>                 _distances         = new();
    private readonly GunRPMCalculator            _gunRPMCalculator;

    private string _aircraftName = string.Empty;

    // 6th root of the minimum and maximum kinetic energy of all guns in the game, brings everything a bit closer together
    private const float MinRtKineticEnergy = 1.318896715f;
    private const float MaxRtKineticEnergy = 3.687686192f;

    // 40 might sound a bit better here, needs more testing
    private const float FreqMin = 35f;
    private const float FreqMax = 80f;


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

    protected override void OnStateDataReceived(StateData stateData)
    {
        for (int i = 0; i < _gunStates.Count; i++)
        {
            var state = _gunStates[i];

            if (Log.IsEnabled(LogEventLevel.Debug))
                _gunRPMCalculator.CalculateIfStopped(stateData.Tick, i);

            if (!state.Firing)
                continue;

            var gun = _guns[i];

            float expectedUpdateRate  = 50f / (gun.RPM / 60f);
            int   maxDiffBetweenShots = (int)Math.Ceiling(expectedUpdateRate);

            if ((int)(stateData.Tick - state.LastFired.Tick) <= maxDiffBetweenShots)
                continue;

            // If too much time has passed since we've received a packet saying this gun is firing, turn it off
            state.Firing     = false;
            state.LastPlayed = default;
        }

        for (int i = 0; i < _gunStates.Count; i++)
        {
            var gun   = _guns[i];
            var state = _gunStates[i];

            // Skip any guns that aren't firing or have been aggregated already
            if (!state.Firing || _aggregated[i])
                continue;

            SimTime playAt;
            if (state.LastPlayed.Tick == 0)
            {
                // If this gun hasn't played an effect yet, queue it up in the future from when it was last fired
                playAt = new SimTime(state.LastFired.AbsoluteTime);
            }
            else if ((int)((stateData.Tick - state.LastPlayed.Tick) * SimClock.SamplesPerTick)
                   - (int)state.LastPlayed.SubTick
                   > gun.SamplesBetweenShots)
            {
                // If this gun has played an effect already and it's time to queue another effect up, add it in the
                // future from when it was last played
                playAt = new SimTime(state.LastPlayed.AbsoluteTime + gun.SamplesBetweenShots);
            }
            else
                continue;

            // Calculate the frequencies and amplitudes for this gun based on its kinetic energy
            float keRatio  = (gun.RtKineticEnergy - MinRtKineticEnergy) / (MaxRtKineticEnergy - MinRtKineticEnergy);
            float freq     = FreqMin + (1 - keRatio) * (FreqMax - FreqMin);
            float ampTotal = CalculateAmplitude(gun.RtKineticEnergy, freq, _distances[i]);

            // Search for any other identical guns that are also firing at the same time and aggregate them
            int aggregatedCount = 1;
            for (int j = i + 1; j < _gunStates.Count; j++)
            {
                var otherState = _gunStates[j];
                if (_aggregated[j]
                 || !state.Firing
                 || gun                           != _guns[j]
                 || state.LastFired.AbsoluteTime  != otherState.LastFired.AbsoluteTime
                 || state.LastPlayed.AbsoluteTime != otherState.LastPlayed.AbsoluteTime)
                    continue;

                // Mark this gun as aggregated, increase count, and set LastPlayed
                _aggregated[j] = true;
                aggregatedCount++;
                otherState.LastPlayed = playAt;

                // Determine the amplitudes this gun would add at full volume given its distance and add to the sum
                ampTotal += CalculateAmplitude(gun.RtKineticEnergy, freq, _distances[j]);
            }

            // Divide the sum by the square root of the number of guns simultaneously firing
            // This gives the following effect
            // 1 gun  - 1.00x
            // 2 guns - 1.19x
            // 3 guns - 1.31x
            // ...
            // 8 guns - 1.68x
            float divisor     = MathF.Pow(aggregatedCount, 6 / 8f);
            float ampWeighted = ampTotal / divisor;
            
            state.LastPlayed = playAt;
            
            // Add the impulse generator for the aggregated guns
            _impulseGenerators.Add(new TimedImpulseGenerator(playAt, freq, ampWeighted, 3, 3));
        }

        // Reset the aggregated array
        for (int i = 0; i < _aggregated.Count; i++)
        {
            _aggregated[i] = false;
        }
    }

    private float CalculateAmplitude(float rtKE, float freq, float distance)
    {
        float amp = rtKE / MaxRtKineticEnergy * Volume.Amplitude;
        // Doesn't make a lot of sense here, gun mounted right under the cockpit is further away than wing mounted due
        // to the exit point at the nose.
        // amp = Attenuate(freq, amp, distance);
        return amp;
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
                    _aggregated.Clear();
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
                    _aggregated.Add(false);
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