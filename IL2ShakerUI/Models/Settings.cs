using System.Diagnostics.CodeAnalysis;
using IL2ShakerDriver;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace IL2ShakerUI.Models;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal class Settings : ReactiveObject, ISettings
{
    [Reactive]
    public string OutputDevice { get; set; }

    [Reactive]
    public int Latency { get; set; }

    public IEffectSettings MasterVolume    { get; set; }
    public IEffectSettings Engine          { get; set; }
    public IEffectSettings LandingGear     { get; set; }
    public IEffectSettings Bumps           { get; set; }
    public IEffectSettings Flaps           { get; set; }
    public IEffectSettings RollRate        { get; set;  }
    public IEffectSettings GForces         { get; set;  }
    public IEffectSettings StallBuffet     { get; set; }
    public IEffectSettings Impacts         { get; set; }
    public IEffectSettings HitsReceived    { get; set; }
    public IEffectSettings Gunfire         { get; set; }
    public IEffectSettings OrdnanceRelease { get; set; }

    [Reactive]
    public bool DebugLogging { get; set; }

    public IEffectSettings LowPassFilter  { get; set; }
    public IEffectSettings HighPassFilter { get; set; }

    public Settings()
    {
        // Need to set it here to avoid ReactiveUI issue
        OutputDevice    = string.Empty;
        MasterVolume    = new EffectSettings();
        Engine          = new EffectSettings();
        LandingGear     = new EffectSettings();
        Bumps           = new EffectSettings();
        Flaps           = new EffectSettings();
        RollRate        = new EffectSettings();
        GForces         = new EffectSettings();
        StallBuffet     = new EffectSettings();
        Impacts         = new EffectSettings();
        HitsReceived    = new EffectSettings();
        Gunfire         = new EffectSettings();
        OrdnanceRelease = new EffectSettings();
        LowPassFilter   = new EffectSettings();
        HighPassFilter  = new EffectSettings();
    }
}