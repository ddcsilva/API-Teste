namespace PersonManagement.Logging.Configuration;

/// <summary>
/// Configurações do sistema de logging
/// </summary>
public class LoggingConfiguration
{
    /// <summary>
    /// Nome da aplicação que aparecerá nos logs
    /// </summary>
    public string ApplicationName { get; set; } = "Application";

    /// <summary>
    /// Ambiente da aplicação (Development, Production, etc.)
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// Diretório onde os arquivos de log serão salvos
    /// </summary>
    public string LogDirectory { get; set; } = "logs";

    /// <summary>
    /// Número de dias para manter os arquivos de log
    /// </summary>
    public int RetainedFileCountLimit { get; set; } = 30;

    /// <summary>
    /// Habilitar logs detalhados para desenvolvimento
    /// </summary>
    public bool EnableDetailedLogging { get; set; }

    /// <summary>
    /// Habilitar log de performance automático
    /// </summary>
    public bool EnablePerformanceLogging { get; set; } = true;

    /// <summary>
    /// Habilitar logs de auditoria
    /// </summary>
    public bool EnableAuditLogging { get; set; } = true;

    /// <summary>
    /// Template para logs no console
    /// </summary>
    public string ConsoleOutputTemplate { get; set; } =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";

    /// <summary>
    /// Template para logs em arquivo
    /// </summary>
    public string FileOutputTemplate { get; set; } =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}";
}
