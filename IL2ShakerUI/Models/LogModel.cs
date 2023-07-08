using System;
using System.IO;
using System.Threading;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace IL2ShakerUI.Models;

internal class LogModel
{
    internal event Action<string>?           LogUpdated;
    private readonly SynchronizationContext? _uiContext = SynchronizationContext.Current;

    public LogModel()
    {
        const string consoleTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Class,-18} {Message:lj}{NewLine}{Exception}";
        const string template        = "[{Timestamp:HH:mm:ss} {Level:u3}] {Class,-18} {Message:lj}{NewLine}{Exception}";

        var formatter        = new MessageTemplateTextFormatter(template);
        var stringWriterSink = new StringWriterSink(this, formatter);
        Logging.LevelSwitch.MinimumLevel = LogEventLevel.Verbose;

        var logFile = new FileInfo(Directory.GetCurrentDirectory() + @"\Log.txt");
        if (logFile.Exists)
            logFile.Delete();
        
        Log.Logger = new LoggerConfiguration().MinimumLevel.ControlledBy(Logging.LevelSwitch)
           .WriteTo.Async(o => o.Console(outputTemplate: consoleTemplate))
           .WriteTo.Async(o => o.Sink(stringWriterSink))
           .WriteTo.Async(o => o.File(formatter, logFile.FullName))
           .CreateLogger();
    }

    public void Update(string message)
    {
        _uiContext?.Post(_ => LogUpdated?.Invoke(message), null);
    }
}