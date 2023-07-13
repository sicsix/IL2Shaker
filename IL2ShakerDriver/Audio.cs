using IL2ShakerDriver.DB;
using IL2ShakerDriver.Effects;
using IL2ShakerDriver.Samplers;
using IL2TelemetryRelay.State;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace IL2ShakerDriver;

internal class Audio : ISampleProvider
{
    public bool       Enabled              { get; private set; }
    public Volume     MasterVolume         { get; private set; }
    public WaveFormat WaveFormat           { get; }
    public SimClock   SimClock             { get; } = new();
    public Database   Database             { get; }
    public MMDevice?  OutputDevice         { get; private set; }
    public int        Latency              { get; private set; }
    public SimSpeed   SimSpeed             { get; private set; }
    public int        TicksAtAbnormalSpeed { get; private set; }
    public object     Lock                 { get; } = new();


    private readonly List<Effect>    _effects = new();
    private readonly ISampleProvider _chainStart;
    private readonly Engine          _engine;
    private readonly LandingGear     _landingGear;
    private readonly Bumps           _bumps;
    private readonly Flaps           _flaps;
    private readonly RollRate        _rollRate;
    private readonly GForces         _gForces;
    private readonly StallBuffet     _stallBuffet;
    private readonly Impacts         _impacts;
    private readonly HitsReceived    _hitsReceived;
    private readonly GunFire         _gunFire;
    private readonly OrdnanceRelease _ordnanceRelease;
    private readonly LowPassFilter   _lowPassFilter;
    private readonly HighPassFilter  _highPassFilter;
    private readonly ISampleProvider _chainEnd;

    public Audio(Database database)
    {
        MasterVolume = new Volume(0);
        WaveFormat   = WaveFormat.CreateIeeeFloatWaveFormat(SimClock.SampleRate, 1);
        Database     = database;

        _chainStart      = new ChainStart(WaveFormat);
        _engine          = new Engine(_chainStart, this);
        _landingGear     = new LandingGear(_engine, this);
        _bumps           = new Bumps(_landingGear, this);
        _flaps           = new Flaps(_bumps, this);
        _rollRate        = new RollRate(_flaps, this);
        _gForces         = new GForces(_rollRate, this);
        _stallBuffet     = new StallBuffet(_gForces, this);
        _impacts         = new Impacts(_stallBuffet, this);
        _hitsReceived    = new HitsReceived(_impacts, this);
        _gunFire         = new GunFire(_hitsReceived, this);
        _ordnanceRelease = new OrdnanceRelease(_gunFire, this);

        _effects = new List<Effect>
        {
            _engine,
            _landingGear,
            _bumps,
            _flaps,
            _rollRate,
            _gForces,
            _stallBuffet,
            _impacts,
            _hitsReceived,
            _gunFire,
            _ordnanceRelease
        };

        _lowPassFilter  = new LowPassFilter(_ordnanceRelease, 80, 2);
        _highPassFilter = new HighPassFilter(_lowPassFilter, 10, 2);

        _chainEnd = _highPassFilter;

        Listener.StateDataReceived += UpdateSimClock;
    }

    public void Start(string? deviceName, int latency, CancellationToken token)
    {
        // TODO need to let caller know if this has failed
        var enumerator = new MMDeviceEnumerator();
        var device = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
           .FirstOrDefault(wasapi => wasapi.FriendlyName == deviceName);

        if (device == null)
        {
            Logging.At(this).Error("Selected output device does not exist");
            return;
        }

        OutputDevice = device;
        using var wo = new WasapiOut(device, AudioClientShareMode.Exclusive, true, latency);

        wo.Init(this);
        wo.Play();

        Logging.At(this).Information("Outputting to '{Output}' with {Latency}ms latency", device.FriendlyName, latency);
        Latency = latency;

        while (wo.PlaybackState == PlaybackState.Playing)
        {
            Thread.Sleep(10);
            if (token.IsCancellationRequested)
                break;
        }
    }

    public static IEnumerable<MMDevice> GetOutputDevices()
    {
        return new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
    }

    public void UpdateSettings(ISettings settings)
    {
        bool  switchedState = settings.MasterVolume.Enabled != Enabled;
        float previousdB    = MasterVolume.dB;

        Enabled      = settings.MasterVolume.Enabled;
        MasterVolume = new Volume(settings.MasterVolume.Value);

        if (switchedState || previousdB != MasterVolume.dB)
            Logging.At(this).Verbose("{State}: {Rel}dB", Enabled ? "Enabled" : "Disabled", MasterVolume.dB);

        _engine.UpdateSettings(settings.Engine);
        _landingGear.UpdateSettings(settings.LandingGear);
        _bumps.UpdateSettings(settings.Bumps);
        _flaps.UpdateSettings(settings.Flaps);
        _rollRate.UpdateSettings(settings.RollRate);
        _gForces.UpdateSettings(settings.GForces);
        _stallBuffet.UpdateSettings(settings.StallBuffet);
        _impacts.UpdateSettings(settings.Impacts);
        _hitsReceived.UpdateSettings(settings.HitsReceived);
        _gunFire.UpdateSettings(settings.Gunfire);
        _ordnanceRelease.UpdateSettings(settings.OrdnanceRelease);
        _lowPassFilter.UpdateSettings(settings.LowPassFilter);
        _highPassFilter.UpdateSettings(settings.HighPassFilter);
    }

    private void UpdateSimClock(StateData stateData)
    {
        lock (Lock)
        {
            var simSpeed = SimClock.UpdateTick(stateData.Tick, Latency, stateData.Paused);
            SetSimSpeed(simSpeed);
        }
    }

    private void SetSimSpeed(SimSpeed simSpeed)
    {
        if (SimSpeed != simSpeed)
        {
            bool   pausing  = simSpeed != SimSpeed.x1 && SimSpeed == SimSpeed.x1;
            bool   resuming = simSpeed == SimSpeed.x1 && SimSpeed != SimSpeed.x1;
            string speed    = simSpeed.ToString().Replace('_', '/');
            string state    = pausing ? " - Pausing shaker output" : resuming ? " - Resuming shaker output" : "";
            Logging.At(this).Debug("Sim speed {Speed}{State}", speed, state);
        }

        TicksAtAbnormalSpeed = simSpeed == SimSpeed.x1 ? 0 : TicksAtAbnormalSpeed + 1;
        SimSpeed             = simSpeed;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samples = count;

        if (Enabled)
            samples = _chainEnd.Read(buffer, offset, count);

        lock (Lock)
        {
            SimClock.Increment(count);
        }

        return samples;
    }

    private class ChainStart : ISampleProvider
    {
        public WaveFormat WaveFormat { get; }

        public ChainStart(WaveFormat waveFormat)
        {
            WaveFormat = waveFormat;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            return count;
        }
    }
}