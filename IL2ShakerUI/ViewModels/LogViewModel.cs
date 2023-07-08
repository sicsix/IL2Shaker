using AvaloniaEdit.Document;
using IL2ShakerUI.Models;

namespace IL2ShakerUI.ViewModels;

internal class LogViewModel : ViewModelBase
{
    public TextDocument? TextDocument { get; }

    private readonly LogModel _logModel;

    public LogViewModel(LogModel logModel)
    {
        TextDocument        =  new TextDocument();
        _logModel           =  logModel;
        logModel.LogUpdated += LogUpdated;
    }

    private void LogUpdated(string message)
    {
        TextDocument?.Insert(TextDocument.TextLength, message);
    }
}