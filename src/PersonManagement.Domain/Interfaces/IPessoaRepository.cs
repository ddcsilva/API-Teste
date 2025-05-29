using PersonManagement.Domain.Entities;

namespace PersonManagement.Domain.Interfaces;

public interface IPessoaRepository
{
    Task<Pessoa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Pessoa?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Pessoa?> ObterPorDocumentoAsync(string documento, CancellationToken cancellationToken = default);
    Task<IEnumerable<Pessoa>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Pessoa>> ObterAtivosAsync(CancellationToken cancellationToken = default);
    Task<Pessoa> AdicionarAsync(Pessoa pessoa, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Pessoa pessoa, CancellationToken cancellationToken = default);
    Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> EmailExisteAsync(string email, Guid? excluirId = null, CancellationToken cancellationToken = default);
    Task<bool> DocumentoExisteAsync(string documento, Guid? excluirId = null, CancellationToken cancellationToken = default);
}