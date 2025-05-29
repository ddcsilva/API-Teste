using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PersonManagement.Logging.Abstractions;
using PersonManagement.Logging.Configuration;
using Serilog;
using Serilog.Events;

namespace PersonManagement.Logging.Extensions;

/// <summary>
/// Extensões para configurar o pipeline de logging no WebApplication
/// </summary>
public static class WebApplicationLoggingExtensions
{
    /// <summary>
    /// Configura o middleware de logging da aplicação
    /// </summary>
    public static WebApplication UseAppLogging(this WebApplication app)
    {
        // Usar o request logging do Serilog
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "🌐 {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = (httpContext, elapsed, ex) => ex != null
                ? LogEventLevel.Error
                : httpContext.Response.StatusCode > 499
                    ? LogEventLevel.Error
                    : httpContext.Response.StatusCode > 399
                        ? LogEventLevel.Warning
                        : LogEventLevel.Information;

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                var userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault();
                if (!string.IsNullOrEmpty(userAgent))
                {
                    diagnosticContext.Set("UserAgent", userAgent);
                }

                if (httpContext.User.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(httpContext.User.Identity.Name))
                {
                    diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
                }
            };
        });

        // Log de inicialização da aplicação
        LogApplicationStartup(app);

        return app;
    }

    private static void LogApplicationStartup(WebApplication app)
    {
        var config = app.Services.GetService<LoggingConfiguration>();
        var logger = app.Services.GetRequiredService<ILogger<WebApplication>>();

        if (app.Environment.IsDevelopment())
        {
            logger.LogInformation("🚀 {ApplicationName} iniciada em modo de DESENVOLVIMENTO", config?.ApplicationName ?? "Aplicação");
            logger.LogInformation("📋 Swagger disponível em: {SwaggerUrl}", "/swagger");
        }
        else
        {
            logger.LogInformation("🚀 {ApplicationName} iniciada em modo de PRODUÇÃO", config?.ApplicationName ?? "Aplicação");
        }

        logger.LogInformation("🌍 Ambiente: {Environment}", app.Environment.EnvironmentName);
        logger.LogInformation("📁 Logs salvos em: {LogDirectory}", config?.LogDirectory ?? "logs");
    }

    /// <summary>
    /// Configura o shutdown graceful do logging
    /// </summary>
    public static void ConfigureLoggingShutdown(this WebApplication app)
    {
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

        lifetime.ApplicationStopping.Register(() =>
        {
            var logger = app.Services.GetService<ILogger<WebApplication>>();
            logger?.LogInformation("🛑 Aplicação sendo finalizada...");

            // Aguardar um pouco para garantir que os logs sejam escritos
            Thread.Sleep(100);

            LoggingServiceCollectionExtensions.CloseAndFlushLogs();
        });
    }
}