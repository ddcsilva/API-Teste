using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PersonManagement.Logging.Abstractions;
using PersonManagement.Logging.Configuration;
using PersonManagement.Logging.Implementation;
using Serilog;
using Serilog.Events;
using SerilogStatic = Serilog.Log;

namespace PersonManagement.Logging.Extensions;

/// <summary>
/// Extensões para configurar o sistema de logging
/// </summary>
public static class LoggingServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona o sistema de logging da aplicação
    /// </summary>
    public static IServiceCollection AddAppLogging(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Configurações do logging
        var loggingConfig = new LoggingConfiguration
        {
            ApplicationName = configuration["ApplicationName"] ?? "PersonManagement",
            Environment = environment.EnvironmentName,
            EnableDetailedLogging = environment.IsDevelopment()
        };

        // Bind configurações customizadas se existirem
        configuration.GetSection("Logging:Custom").Bind(loggingConfig);

        services.AddSingleton(loggingConfig);

        // Configurar Serilog
        ConfigureSerilog(loggingConfig, configuration);

        // Adicionar Serilog ao pipeline
        services.AddSerilog();

        // Registrar nossas abstrações
        services.AddScoped(typeof(IAppLogger<>), typeof(SerilogAppLogger<>));
        services.AddScoped<IAppLogger, SerilogAppLogger>();

        return services;
    }

    /// <summary>
    /// Adiciona o sistema de logging com configuração customizada
    /// </summary>
    public static IServiceCollection AddAppLogging(this IServiceCollection services, LoggingConfiguration loggingConfig)
    {
        services.AddSingleton(loggingConfig);

        // Configurar Serilog
        ConfigureSerilog(loggingConfig);

        // Adicionar Serilog ao pipeline
        services.AddSerilog();

        // Registrar nossas abstrações
        services.AddScoped(typeof(IAppLogger<>), typeof(SerilogAppLogger<>));
        services.AddScoped<IAppLogger, SerilogAppLogger>();

        return services;
    }

    private static void ConfigureSerilog(LoggingConfiguration config, IConfiguration? configuration = null)
    {
        var loggerConfig = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", config.ApplicationName)
            .Enrich.WithProperty("Environment", config.Environment);

        // Se temos configuração do appsettings, usar ela também
        if (configuration != null)
        {
            loggerConfig = loggerConfig.ReadFrom.Configuration(configuration);
        }

        // Console output
        loggerConfig = loggerConfig.WriteTo.Console(
            outputTemplate: config.ConsoleOutputTemplate);

        // File output
        var logFilePath = Path.Combine(config.LogDirectory, "app-.log");
        loggerConfig = loggerConfig.WriteTo.File(
            logFilePath,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: config.RetainedFileCountLimit,
            outputTemplate: config.FileOutputTemplate);

        // Configurar níveis baseado no ambiente
        if (config.EnableDetailedLogging)
        {
            loggerConfig = loggerConfig.MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information);
        }
        else
        {
            loggerConfig = loggerConfig.MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning);
        }

        SerilogStatic.Logger = loggerConfig.CreateLogger();
    }

    /// <summary>
    /// Finaliza o sistema de logging de forma segura
    /// </summary>
    public static void CloseAndFlushLogs()
    {
        SerilogStatic.Information("🛑 Sistema de logging sendo finalizado...");
        SerilogStatic.CloseAndFlush();
    }
}