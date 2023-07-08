using IL2ShakerDriver.DB;

namespace IL2ShakerDriver;

public class Driver : IDisposable
{
    private readonly Database _db;
    private readonly Audio    _audio;

    private CancellationTokenSource? _listenerToken;
    private Task?                    _listenerTask;

    private CancellationTokenSource? _audioToken;
    private Task?                    _audioTask;
    private int                      _currentLatency;

    public Driver()
    {
        _db    = new Database();
        _audio = new Audio(_db);
    }

    public async void Dispose()
    {
        _audioToken?.Cancel();
        _listenerToken?.Cancel();
        if (_listenerTask != null)
        {
            await _listenerTask;
            _listenerTask.Dispose();
        }

        if (_audioTask != null)
        {
            await _audioTask;
            _audioTask.Dispose();
        }
    }

    public void Initialise()
    {
        Logging.At(typeof(Driver)).Information("IL2ShakerDriver initialising...");
        _db.LoadFiles();
    }

    public void BeginListening()
    {
        if (_listenerTask != null)
            throw new InvalidOperationException("Listener already running");
        // TODO Need to know if this fails
        _listenerToken = new CancellationTokenSource();
        _listenerTask  = Task.Run(() => Listener.ListenToStream(_listenerToken.Token));
    }

    private void Start(string? deviceName, int latency)
    {
        _audioToken = new CancellationTokenSource();
        // TODO Need to know if this fails
        _audioTask = Task.Run(() => _audio.Start(deviceName, latency, _audioToken.Token));
    }

    private async void Stop()
    {
        _audioToken?.Cancel();
        if (_audioTask != null)
            await _audioTask;
    }

    public async void UpdateSettings(ISettings settings)
    {
        _audio.UpdateSettings(settings);
        if (_audio.OutputDevice              == null
         || _audio.OutputDevice.FriendlyName != settings.OutputDevice
         || _currentLatency                  != settings.Latency)
        {
            await Task.Run(Stop);
            _currentLatency = settings.Latency;
            Start(settings.OutputDevice, settings.Latency);
        }
    }

    public static IEnumerable<string> GetOutputDevices()
    {
        return Audio.GetOutputDevices().Select(o => o.FriendlyName);
    }
}