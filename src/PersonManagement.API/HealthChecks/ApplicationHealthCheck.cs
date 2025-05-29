using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using PersonManagement.Domain.Interfaces;

namespace PersonManagement.API.HealthChecks;

public class ApplicationHealthCheck : IHealthCheck
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApplicationHealthCheck> _logger;

    public ApplicationHealthCheck(IUnitOfWork unitOfWork, ILogger<ApplicationHealthCheck> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar conectividade com o banco de dados
            var totalPessoas = await _unitOfWork.PessoaRepository.ObterTodosAsync(cancellationToken: cancellationToken);
            var pessoasCount = totalPessoas?.Count() ?? 0;

            // Dados adicionais para o health check
            var data = new Dictionary<string, object>
            {
                { "TotalPersons", pessoasCount },
                { "LastChecked", DateTime.UtcNow },
                { "DatabaseProvider", GetDatabaseProvider() },
                { "ThreadPoolThreads", ThreadPool.ThreadCount },
                { "WorkingSet", Environment.WorkingSet / 1024 / 1024 } // MB
            };

            // Verificações adicionais
            if (pessoasCount >= 0) // Se conseguiu fazer a query
            {
                return HealthCheckResult.Healthy(
                    $"Application is running correctly. Database has {pessoasCount} persons registered.",
                    data);
            }

            return HealthCheckResult.Degraded(
                "Application is running but with issues accessing data",
                data: data);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Health check was cancelled");
            return HealthCheckResult.Degraded(
                "Health check was cancelled",
                data: new Dictionary<string, object> { { "CancelledAt", DateTime.UtcNow } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed with exception");

            return HealthCheckResult.Unhealthy(
                "Application health check failed",
                ex,
                new Dictionary<string, object>
                {
                    { "Exception", ex.Message },
                    { "FailedAt", DateTime.UtcNow },
                    { "ExceptionType", ex.GetType().Name }
                });
        }
    }

    private string GetDatabaseProvider()
    {
        try
        {
            // Tentativa de obter o provider do banco através do contexto
            // Isso é uma implementação simplificada
            return "Entity Framework Provider";
        }
        catch
        {
            return "Unknown";
        }
    }
}