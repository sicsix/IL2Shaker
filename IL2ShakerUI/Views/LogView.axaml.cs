using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaEdit;

namespace IL2ShakerUI.Views;

public partial class LogView : UserControl
{
    private readonly TextEditor _textEditor;

    public LogView()
    {
        InitializeComponent();

        _textEditor                           =  this.FindControl<TextEditor>("TextEditor");
        _textEditor.TextArea.Caret.CaretBrush =  Brushes.Transparent;
        _textEditor.TextChanged               += (o, _) => ((TextEditor)o!).ScrollToEnd();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _textEditor.ScrollToEnd();
    }
}