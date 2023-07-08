using IL2ShakerUI.Models;

namespace IL2ShakerUI.ViewModels;

internal class MainWindowViewModel : ViewModelBase
{
    public LogViewModel      LogViewModel      { get; private set; }
    public SettingsViewModel SettingsViewModel { get; private set; }

    public MainWindowViewModel(LogViewModel logViewModel, SettingsViewModel settingsViewModel)
    {
        LogViewModel      = logViewModel;
        SettingsViewModel = settingsViewModel;
    }

    public MainWindowViewModel()
    {
        LogViewModel      = new LogViewModel(new LogModel());
        SettingsViewModel = new SettingsViewModel(new DriverModel());
    }
}