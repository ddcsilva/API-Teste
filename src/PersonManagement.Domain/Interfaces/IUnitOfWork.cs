namespace PersonManagement.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IPessoaRepository PessoaRepository { get; }
    Task<int> SalvarMudancasAsync(CancellationToken cancellationToken = default);
    Task IniciarTransacaoAsync(CancellationToken cancellationToken = default);
    Task ConfirmarTransacaoAsync(CancellationToken cancellationToken = default);
    Task ReverterTransacaoAsync(CancellationToken cancellationToken = default);
}