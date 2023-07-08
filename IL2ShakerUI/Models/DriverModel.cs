using System;
using System.Collections.Generic;
using System.IO;
using IL2ShakerDriver;
using Serilog.Events;
using YamlDotNet.Serialization;

namespace IL2ShakerUI.Models;

internal class DriverModel : IDisposable
{
    internal readonly Settings Settings;
    private readonly  Driver   _driver;

    public IEnumerable<string> GetOutputDevices()
    {
        return Driver.GetOutputDevices();
    }

    public DriverModel()
    {
        Logging.At(this).Information("Parsing settings...");
        Settings                         = LoadSettings(@"\Settings.yaml");
        Logging.LevelSwitch.MinimumLevel = Settings.DebugLogging ? LogEventLevel.Verbose : LogEventLevel.Information;
        _driver                          = new Driver();
    }

    public void Dispose()
    {
        _driver.Dispose();
    }

    private static Settings LoadSettings(string filename)
    {
        using TextReader reader = new StreamReader(Directory.GetCurrentDirectory() + filename);
        var deserializer = new DeserializerBuilder().WithTypeMapping<IEffectSettings, EffectSettings>().Build();
        return deserializer.Deserialize<Settings>(reader);
    }

    public void Initialise()
    {
        _driver.Initialise();
        _driver.BeginListening();
        UpdateSettings();
    }

    public void UpdateSettings()
    {
        _driver.UpdateSettings(Settings);
    }

    public void RevertSettings()
    {
        var newSettings = LoadSettings(@"\DefaultSettings.yaml");
        CopySettings(Settings, newSettings);
    }

    public void UndoSettings()
    {
        var newSettings = LoadSettings(@"\Settings.yaml");
        CopySettings(Settings, newSettings);
    }

    public void SaveSettings()
    {
        try
        {
            Logging.At(this).Debug("Saving settings...");
            using TextWriter writer = new StreamWriter(Directory.GetCurrentDirectory() + @"\Settings.yaml");
            var serializer = new SerializerBuilder().WithTypeInspector(o => new WriteableTypeInspector(o)).Build();
            serializer.Serialize(writer, Settings);
        }
        catch
        {
            Logging.At(this).Error("Failed to save settings to disk");
        }
    }

    private static void CopySettings(Settings dst, Settings src)
    {
        dst.OutputDevice = src.OutputDevice;
        dst.Latency      = src.Latency;
        CopyEffectSettings(dst.MasterVolume, src.MasterVolume);
        CopyEffectSettings(dst.Engine, src.Engine);
        CopyEffectSettings(dst.LandingGear, src.LandingGear);
        CopyEffectSettings(dst.Bumps, src.Bumps);
        CopyEffectSettings(dst.Flaps, src.Flaps);
        CopyEffectSettings(dst.StallBuffet, src.StallBuffet);
        CopyEffectSettings(dst.Impacts, src.Impacts);
        CopyEffectSettings(dst.HitsReceived, src.HitsReceived);
        CopyEffectSettings(dst.Gunfire, src.Gunfire);
        CopyEffectSettings(dst.OrdnanceRelease, src.OrdnanceRelease);
        dst.DebugLogging = src.DebugLogging;
        CopyEffectSettings(dst.LowPassFilter, src.LowPassFilter);
        CopyEffectSettings(dst.HighPassFilter, src.HighPassFilter);
    }

    private static void CopyEffectSettings(IEffectSettings dst, IEffectSettings src)
    {
        dst.Enabled = src.Enabled;
        dst.Value   = src.Value;
    }
}