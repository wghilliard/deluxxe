// sourced from https://khalidabuhakmeh.com/logging-trace-output-using-ilogger-in-dotnet-applications

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Deluxxe.Extensions;

public class LoggerTraceListener(ILoggerFactory loggerFactory) : TraceListener
{
    private readonly ILogger _defaultLogger = loggerFactory.CreateLogger(nameof(LoggerTraceListener));
    private readonly ConcurrentDictionary<string, ILogger> _loggers = new();
    private readonly StringBuilder _builder = new();

    public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? message)
    {
        var logger = _loggers.GetOrAdd(
            source,
            static (s, factory) => factory.CreateLogger(s),
            loggerFactory);
        
        logger.Log(MapLevel(eventType), message);
    }

    public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? format,
        params object?[]? args)
    {
        var logger = _loggers.GetOrAdd(
            source,
            static (s, factory) => factory.CreateLogger(s),
            loggerFactory);
        
        logger.Log(MapLevel(eventType), format, args ?? Array.Empty<object>());
    }

    public override void Write(string? message)
    {
        _builder.Append(message);
    }

    public override void WriteLine(string? message)
    {
        _builder.AppendLine(message);
        _defaultLogger.LogInformation(_builder.ToString());
        _builder.Clear();
    }

    private LogLevel MapLevel(TraceEventType eventType) => eventType switch
    {
        TraceEventType.Verbose => LogLevel.Debug,
        TraceEventType.Information => LogLevel.Information,
        TraceEventType.Critical => LogLevel.Critical,
        TraceEventType.Error => LogLevel.Error,
        TraceEventType.Warning => LogLevel.Warning,
        _ => LogLevel.Trace
    };
}
