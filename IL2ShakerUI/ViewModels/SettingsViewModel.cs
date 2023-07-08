using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Input;
using DynamicData.Binding;
using IL2ShakerUI.Models;
using ReactiveUI;
using Serilog.Events;

namespace IL2ShakerUI.ViewModels;

internal class SettingsViewModel : ViewModelBase
{
    internal         Settings    Settings { get; }
    private readonly DriverModel _driverModel;

    public IEnumerable<string> OutputDevices => _driverModel.GetOutputDevices();

    public SettingsViewModel(DriverModel driverModel)
    {
        _driverModel  = driverModel;
        Settings      = _driverModel.Settings;
        RevertCommand = ReactiveCommand.Create(Revert);
        UndoCommand   = ReactiveCommand.Create(Undo);
        SaveCommand   = ReactiveCommand.Create(Save);
        Settings.WhenPropertyChanged(o => o.OutputDevice, false).Subscribe(o => OnDeviceChanged(o.Sender));
        Settings.WhenPropertyChanged(o => o.Latency, false).Throttle(TimeSpan.FromSeconds(0.5))
           .Subscribe(o => OnLatencyChanged(o.Sender));
        Settings.WhenPropertyChanged(o => o.DebugLogging, false).Subscribe(o => OnLoggingChanged(o.Sender));
        ((EffectSettings)Settings.MasterVolume).WhenAnyPropertyChanged().Subscribe(OnSettingsChanged!);
        ((EffectSettings)Settings.Engine).WhenAnyPropertyChanged().Subscribe(OnSettingsChanged!);
        ((EffectSettings)Settings.LandingGear).WhenAnyPropertyChanged().Subscribe(OnSettingsChanged!);
        ((EffectSettings)Settings.Bumps).WhenAnyPropertyChanged().Subscribe(OnSettingsChanged!);
        ((EffectSettings)Settings.Flaps).WhenAnyPropertyChanged().Subscribe(OnSettingsChanged!);
        ((EffectSettings)Settings.StallBuffet).WhenAnyPropertyChanged().Subscribe(OnSettingsChanged!);
        ((EffectSettings)Settings.Impacts).WhenAnyPropertyChanged().Subscribe(OnSettingsChanged!);
        ((EffectSettings)Settings.HitsReceived).WhenAnyPropertyChanged().Subscribe(OnSettingsChanged!);
        ((EffectSettings)Settings.Gunfire).WhenAnyPropertyChanged().Subscribe(OnSettingsChanged!);
        ((EffectSettings)Settings.OrdnanceRelease).WhenAnyPropertyChanged().Subscribe(OnSettingsChanged!);
        ((EffectSettings)Settings.LowPassFilter).WhenAnyPropertyChanged().Subscribe(OnSettingsChanged!);
        ((EffectSettings)Settings.HighPassFilter).WhenAnyPropertyChanged().Subscribe(OnSettingsChanged!);
    }

    private void OnDeviceChanged(Settings settings)
    {
        _driverModel.UpdateSettings();
    }

    private void OnLatencyChanged(Settings settings)
    {
        _driverModel.UpdateSettings();
    }

    private static void OnLoggingChanged(Settings settings)
    {
        Logging.LevelSwitch.MinimumLevel = settings.DebugLogging ? LogEventLevel.Verbose : LogEventLevel.Information;
    }

    private void OnSettingsChanged(EffectSettings settings)
    {
        _driverModel.UpdateSettings();
    }

    public ICommand RevertCommand { get; }

    private void Revert()
    {
        _driverModel.RevertSettings();
        _driverModel.UpdateSettings();
    }

    public ICommand UndoCommand { get; }

    private void Undo()
    {
        _driverModel.UndoSettings();
        _driverModel.UpdateSettings();
    }

    public ICommand SaveCommand { get; }

    private void Save()
    {
        _driverModel.SaveSettings();
    }
}