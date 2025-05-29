using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using PersonManagement.Infrastructure.Data;
using System.Text.Json;

namespace PersonManagement.API.Extensions;

/// <summary>
/// Extensões para configurar Health Checks da aplicação
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adiciona health checks da aplicação
    /// </summary>
    public static IServiceCollection AddApplicationHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>(
                name: "database",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "db", "sql", "database" })
            .AddCheck<CustomHealthCheck>(
                name: "application-health",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "custom", "application" })
            .AddCheck("memory", () =>
            {
                var allocatedBytes = GC.GetTotalMemory(forceFullCollection: false);
                var allocatedMB = allocatedBytes / 1024 / 1024;

                return allocatedMB < 1024
                    ? HealthCheckResult.Healthy($"Memória utilizada: {allocatedMB} MB")
                    : HealthCheckResult.Degraded($"Alto uso de memória: {allocatedMB} MB");
            },
            tags: new[] { "memory", "system" });

        return services;
    }

    /// <summary>
    /// Configura endpoints de health checks
    /// </summary>
    public static WebApplication ConfigureHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponse
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteHealthCheckResponse
        });

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false // Apenas verifica se a aplicação está respondendo
        });

        return app;
    }

    private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var result = new
        {
            Status = report.Status.ToString(),
            CheckedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
            TotalDuration = report.TotalDuration.TotalMilliseconds.ToString("F2") + "ms",
            Environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            Application = "PersonManagement",
            Version = "1.0.0",
            Checks = report.Entries.Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Description = entry.Value.Description ?? "Sem descrição",
                Duration = entry.Value.Duration.TotalMilliseconds.ToString("F2") + "ms",
                Exception = entry.Value.Exception?.Message,
                Tags = entry.Value.Tags?.ToArray() ?? Array.Empty<string>()
            }).ToArray()
        };

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Verificação de saúde do banco de dados
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(ApplicationDbContext context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Tentar fazer uma operação simples no banco
            await _context.Database.CanConnectAsync(cancellationToken);

            var data = new Dictionary<string, object>
            {
                { "DatabaseProvider", _context.Database.ProviderName ?? "Unknown" },
                { "LastCheck", DateTime.UtcNow }
            };

            return HealthCheckResult.Healthy("Conexão com banco de dados funcionando", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar saúde do banco de dados");

            return HealthCheckResult.Unhealthy(
                "Erro na conexão com banco de dados",
                ex,
                new Dictionary<string, object> { { "Error", ex.Message } });
        }
    }
}

/// <summary>
/// Verificação de saúde customizada da aplicação
/// </summary>
public class CustomHealthCheck : IHealthCheck
{
    private readonly ILogger<CustomHealthCheck> _logger;
    private readonly ApplicationDbContext _context;

    public CustomHealthCheck(ILogger<CustomHealthCheck> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar se consegue conectar ao banco e contar registros
            var totalPessoas = await _context.Pessoas.CountAsync(cancellationToken);

            // Verificar espaço em disco (se necessário)
            var freeSpaceInMB = GetFreeSpaceInMB();

            var data = new Dictionary<string, object>
            {
                { "TotalPessoas", totalPessoas },
                { "FreeSpaceMB", freeSpaceInMB },
                { "LastCheck", DateTime.UtcNow }
            };

            if (freeSpaceInMB < 100) // Menos de 100MB
            {
                return HealthCheckResult.Degraded(
                    "Pouco espaço em disco disponível",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                "Aplicação funcionando corretamente",
                data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante verificação de saúde customizada");

            return HealthCheckResult.Unhealthy(
                "Erro na verificação de saúde da aplicação",
                ex,
                new Dictionary<string, object> { { "Error", ex.Message } });
        }
    }

    private static long GetFreeSpaceInMB()
    {
        try
        {
            var drive = new DriveInfo(Directory.GetCurrentDirectory());
            return drive.AvailableFreeSpace / 1024 / 1024; // Converter para MB
        }
        catch
        {
            return -1; // Não foi possível verificar
        }
    }
}