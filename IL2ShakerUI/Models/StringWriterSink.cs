using System;
using System.IO;
using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace IL2ShakerUI.Models;

internal class StringWriterSink : ILogEventSink
{
    private readonly LogModel       _logModel;
    private readonly StringWriter   _writer = new();
    private readonly StringBuilder  _builder;
    private readonly ITextFormatter _formatter;

    public StringWriterSink(LogModel logModel, ITextFormatter formatter)
    {
        _logModel  = logModel;
        _formatter = formatter;
        _builder   = _writer.GetStringBuilder();
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent == null)
            throw new ArgumentNullException(nameof(logEvent));

        _formatter.Format(logEvent, _writer);
        _logModel.Update(_builder.ToString());
        _builder.Clear();
    }
}