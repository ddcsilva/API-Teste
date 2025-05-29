using Microsoft.Extensions.Diagnostics.HealthChecks;
using PersonManagement.Domain.Interfaces;

namespace PersonManagement.API.HealthChecks;

public class ApplicationHealthCheck : IHealthCheck
{
    private readonly IUnitOfWork _unitOfWork;

    public ApplicationHealthCheck(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verifica se consegue acessar o repositório
            var canAccess = await _unitOfWork.PessoaRepository.ObterTodosAsync(cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy("Aplicação funcionando corretamente");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Erro na aplicação", ex);
        }
    }
}