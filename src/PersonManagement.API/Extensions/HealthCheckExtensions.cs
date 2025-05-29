using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PersonManagement.API.HealthChecks;
using PersonManagement.Infrastructure.Data;
using System.Diagnostics;
using System.Text.Json;

namespace PersonManagement.API.Extensions;

/// <summary>
/// Extensions for configuring application Health Checks
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Add comprehensive health checks to the application
    /// </summary>
    public static IServiceCollection AddAdvancedHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthBuilder = services.AddHealthChecks();

        // Database Health Check
        var databaseType = configuration.GetValue<string>("DatabaseType", "Sqlite");
        switch (databaseType?.ToLower())
        {
            case "sqlite":
                healthBuilder.AddSqlite(
                    configuration.GetConnectionString("SqliteConnection") ?? "Data Source=PersonManagement.db",
                    name: "database",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "db", "sql", "sqlite", "ready" });
                break;

            case "sqlserver":
                healthBuilder.AddSqlServer(
                    configuration.GetConnectionString("DefaultConnection") ?? string.Empty,
                    name: "database",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "db", "sql", "sqlserver", "ready" });
                break;

            default:
                // For in-memory databases or unknown types, just add the application health check
                // The ApplicationHealthCheck will handle database verification
                break;
        }

        // Custom Application Health Check
        healthBuilder.AddCheck<ApplicationHealthCheck>(
            name: "application",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "application", "custom", "ready" });

        // Memory Health Check
        healthBuilder.AddCheck("memory", () =>
        {
            var allocatedBytes = GC.GetTotalMemory(forceFullCollection: false);
            var allocatedMB = allocatedBytes / 1024 / 1024;

            var data = new Dictionary<string, object>
            {
                { "AllocatedMemoryMB", allocatedMB },
                { "Gen0Collections", GC.CollectionCount(0) },
                { "Gen1Collections", GC.CollectionCount(1) },
                { "Gen2Collections", GC.CollectionCount(2) }
            };

            if (allocatedMB < 512)
                return HealthCheckResult.Healthy($"Memory usage: {allocatedMB} MB", data);

            if (allocatedMB < 1024)
                return HealthCheckResult.Degraded($"High memory usage: {allocatedMB} MB", null, data);

            return HealthCheckResult.Unhealthy($"Critical memory usage: {allocatedMB} MB", null, data);
        },
        tags: new[] { "memory", "system", "ready" });

        // Disk Space Health Check
        healthBuilder.AddCheck("disk-space", () =>
        {
            try
            {
                var drive = new DriveInfo(Directory.GetCurrentDirectory());
                var freeSpaceGB = drive.AvailableFreeSpace / 1024 / 1024 / 1024;
                var totalSpaceGB = drive.TotalSize / 1024 / 1024 / 1024;
                var usedPercentage = ((double)(totalSpaceGB - freeSpaceGB) / totalSpaceGB) * 100;

                var data = new Dictionary<string, object>
                {
                    { "FreeSpaceGB", freeSpaceGB },
                    { "TotalSpaceGB", totalSpaceGB },
                    { "UsedPercentage", Math.Round(usedPercentage, 2) }
                };

                if (freeSpaceGB > 5) // More than 5GB free
                    return HealthCheckResult.Healthy($"Disk space: {freeSpaceGB}GB free ({usedPercentage:F1}% used)", data);

                if (freeSpaceGB > 1) // More than 1GB free
                    return HealthCheckResult.Degraded($"Low disk space: {freeSpaceGB}GB free ({usedPercentage:F1}% used)", null, data);

                return HealthCheckResult.Unhealthy($"Critical disk space: {freeSpaceGB}GB free ({usedPercentage:F1}% used)", null, data);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Degraded($"Could not check disk space: {ex.Message}");
            }
        },
        tags: new[] { "disk", "system" });

        // System Uptime Check
        healthBuilder.AddCheck("uptime", () =>
        {
            var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            var data = new Dictionary<string, object>
            {
                { "UptimeSeconds", Math.Round(uptime.TotalSeconds, 2) },
                { "UptimeFormatted", $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s" }
            };

            return HealthCheckResult.Healthy($"Application uptime: {data["UptimeFormatted"]}", data);
        },
        tags: new[] { "uptime", "system" });

        return services;
    }

    /// <summary>
    /// Configure health check endpoints with different purposes
    /// </summary>
    public static WebApplication ConfigureAdvancedHealthChecks(this WebApplication app)
    {
        // Detailed health check - all information
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteDetailedHealthResponse
        });

        // Readiness probe - checks if app is ready to serve traffic
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteDetailedHealthResponse
        });

        // Liveness probe - basic check if app is alive
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false, // Only checks if the app responds
            ResponseWriter = WriteSimpleHealthResponse
        });

        // Startup probe - checks if app has started correctly
        app.MapHealthChecks("/health/startup", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteSimpleHealthResponse
        });

        return app;
    }

    /// <summary>
    /// Write detailed health check response with full information
    /// </summary>
    private static async Task WriteDetailedHealthResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var result = new
        {
            Status = report.Status.ToString(),
            CheckedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
            TotalDuration = $"{report.TotalDuration.TotalMilliseconds:F2}ms",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            Application = "PersonManagement API",
            Version = "1.0.0",
            MachineName = Environment.MachineName,
            ProcessId = Environment.ProcessId,
            Checks = report.Entries.Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Description = entry.Value.Description ?? "No description",
                Duration = $"{entry.Value.Duration.TotalMilliseconds:F2}ms",
                Exception = entry.Value.Exception?.Message,
                Data = entry.Value.Data?.Count > 0 ? entry.Value.Data : null,
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

    /// <summary>
    /// Write simple health check response for liveness/startup probes
    /// </summary>
    private static async Task WriteSimpleHealthResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var result = new
        {
            Status = report.Status.ToString(),
            CheckedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
            Application = "PersonManagement API"
        };

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}