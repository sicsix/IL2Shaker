using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace IL2ShakerUI.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}