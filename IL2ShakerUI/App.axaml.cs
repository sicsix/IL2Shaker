using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using IL2ShakerUI.Models;
using IL2ShakerUI.ViewModels;
using IL2ShakerUI.Views;
using Serilog;

namespace IL2ShakerUI;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var logModel     = new LogModel();
            var logViewModel = new LogViewModel(logModel);
            var driverModel  = new DriverModel();

            desktop.ShutdownRequested += (_, _) =>
            {
                driverModel.Dispose();
                Log.CloseAndFlush();
            };

            driverModel.Initialise();
            var settingsViewModel = new SettingsViewModel(driverModel);

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(logViewModel, settingsViewModel)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}