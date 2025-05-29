using Microsoft.Extensions.Logging;
using PersonManagement.Logging.Abstractions;
using Serilog;
using SerilogStatic = Serilog.Log;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace PersonManagement.Logging.Implementation;

/// <summary>
/// Implementação do IAppLogger usando Serilog
/// </summary>
public class SerilogAppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;
    private readonly Serilog.ILogger _serilogLogger;

    public SerilogAppLogger(ILogger<T> logger)
    {
        _logger = logger;
        _serilogLogger = SerilogStatic.ForContext<T>();
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state);
    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _logger.Log(logLevel, eventId, state, exception, formatter);
    }

    public void LogWithContext(LogLevel level, string messageTemplate, params object[] propertyValues)
    {
        var serilogLevel = ConvertLogLevel(level);
        _serilogLogger.Write(serilogLevel, messageTemplate, propertyValues);
    }

    public void LogPerformance(string operation, TimeSpan duration, object? context = null)
    {
        _serilogLogger
            .ForContext("Operation", operation)
            .ForContext("Duration", duration.TotalMilliseconds)
            .ForContext("Context", context, true)
            .Information("⏱️ {Operation} executado em {DurationFormatted}", operation, $"{duration.TotalMilliseconds:F2}ms");
    }

    public void LogBusinessError(string operation, string errorMessage, object? context = null)
    {
        _serilogLogger
            .ForContext("Operation", operation)
            .ForContext("ErrorType", "Business")
            .ForContext("Context", context, true)
            .Warning("⚠️ Erro de negócio em {Operation}: {ErrorMessage}", operation, errorMessage);
    }

    public void LogAudit(string action, string user, object? context = null)
    {
        _serilogLogger
            .ForContext("AuditAction", action)
            .ForContext("User", user)
            .ForContext("Context", context, true)
            .Information("📋 Auditoria: {User} executou {Action}", user, action);
    }

    private static Serilog.Events.LogEventLevel ConvertLogLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => Serilog.Events.LogEventLevel.Verbose,
            LogLevel.Debug => Serilog.Events.LogEventLevel.Debug,
            LogLevel.Information => Serilog.Events.LogEventLevel.Information,
            LogLevel.Warning => Serilog.Events.LogEventLevel.Warning,
            LogLevel.Error => Serilog.Events.LogEventLevel.Error,
            LogLevel.Critical => Serilog.Events.LogEventLevel.Fatal,
            _ => Serilog.Events.LogEventLevel.Information
        };
    }
}

/// <summary>
/// Implementação não tipada do IAppLogger
/// </summary>
public class SerilogAppLogger : IAppLogger
{
    private readonly ILogger _logger;
    private readonly Serilog.ILogger _serilogLogger;

    public SerilogAppLogger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("PersonManagement");
        _serilogLogger = SerilogStatic.Logger;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state);
    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _logger.Log(logLevel, eventId, state, exception, formatter);
    }

    public void LogWithContext(LogLevel level, string messageTemplate, params object[] propertyValues)
    {
        var serilogLevel = ConvertLogLevel(level);
        _serilogLogger.Write(serilogLevel, messageTemplate, propertyValues);
    }

    public void LogPerformance(string operation, TimeSpan duration, object? context = null)
    {
        _serilogLogger
            .ForContext("Operation", operation)
            .ForContext("Duration", duration.TotalMilliseconds)
            .ForContext("Context", context, true)
            .Information("⏱️ {Operation} executado em {DurationFormatted}", operation, $"{duration.TotalMilliseconds:F2}ms");
    }

    public void LogBusinessError(string operation, string errorMessage, object? context = null)
    {
        _serilogLogger
            .ForContext("Operation", operation)
            .ForContext("ErrorType", "Business")
            .ForContext("Context", context, true)
            .Warning("⚠️ Erro de negócio em {Operation}: {ErrorMessage}", operation, errorMessage);
    }

    public void LogAudit(string action, string user, object? context = null)
    {
        _serilogLogger
            .ForContext("AuditAction", action)
            .ForContext("User", user)
            .ForContext("Context", context, true)
            .Information("📋 Auditoria: {User} executou {Action}", user, action);
    }

    private static Serilog.Events.LogEventLevel ConvertLogLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => Serilog.Events.LogEventLevel.Verbose,
            LogLevel.Debug => Serilog.Events.LogEventLevel.Debug,
            LogLevel.Information => Serilog.Events.LogEventLevel.Information,
            LogLevel.Warning => Serilog.Events.LogEventLevel.Warning,
            LogLevel.Error => Serilog.Events.LogEventLevel.Error,
            LogLevel.Critical => Serilog.Events.LogEventLevel.Fatal,
            _ => Serilog.Events.LogEventLevel.Information
        };
    }
}