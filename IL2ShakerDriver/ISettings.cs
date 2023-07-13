namespace IL2ShakerDriver;

public interface ISettings
{
    public string?         OutputDevice    { get; }
    public int             Latency         { get; }
    public IEffectSettings MasterVolume    { get; }
    public IEffectSettings Engine          { get; }
    public IEffectSettings LandingGear     { get; }
    public IEffectSettings Bumps           { get; }
    public IEffectSettings Flaps           { get; }
    public IEffectSettings RollRate        { get; }
    public IEffectSettings GForces         { get; }
    public IEffectSettings StallBuffet     { get; }
    public IEffectSettings Impacts         { get; }
    public IEffectSettings HitsReceived    { get; }
    public IEffectSettings Gunfire         { get; }
    public IEffectSettings OrdnanceRelease { get; }
    public bool            DebugLogging    { get; }
    public IEffectSettings LowPassFilter   { get; }
    public IEffectSettings HighPassFilter  { get; }
}