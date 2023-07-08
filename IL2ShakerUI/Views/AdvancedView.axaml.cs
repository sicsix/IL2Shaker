using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace IL2ShakerUI.Views;

public partial class AdvancedView : UserControl
{
    public AdvancedView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}