using Microsoft.Extensions.Logging;

namespace PersonManagement.Logging.Abstractions;

/// <summary>
/// Interface para logging da aplicação.
/// Abstrai a implementação específica do Serilog, permitindo troca futura.
/// </summary>
public interface IAppLogger<out T> : ILogger<T>
{
    /// <summary>
    /// Log com contexto estruturado
    /// </summary>
    void LogWithContext(LogLevel level, string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Log de performance de operação
    /// </summary>
    void LogPerformance(string operation, TimeSpan duration, object? context = null);

    /// <summary>
    /// Log de erro de negócio
    /// </summary>
    void LogBusinessError(string operation, string errorMessage, object? context = null);

    /// <summary>
    /// Log de auditoria
    /// </summary>
    void LogAudit(string action, string user, object? context = null);
}

/// <summary>
/// Interface genérica para logging sem tipagem
/// </summary>
public interface IAppLogger : ILogger
{
    void LogWithContext(LogLevel level, string messageTemplate, params object[] propertyValues);
    void LogPerformance(string operation, TimeSpan duration, object? context = null);
    void LogBusinessError(string operation, string errorMessage, object? context = null);
    void LogAudit(string action, string user, object? context = null);
}