using Microsoft.EntityFrameworkCore;
using PersonManagement.Domain.Entities;
using PersonManagement.Domain.Interfaces;
using PersonManagement.Infrastructure.Data;

namespace PersonManagement.Infrastructure.Repositories;

public class PessoaRepository : IPessoaRepository
{
    private readonly ApplicationDbContext _context;

    public PessoaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Pessoa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Pessoas.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Pessoa?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Pessoas.FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
    }

    public async Task<Pessoa?> ObterPorDocumentoAsync(string documento, CancellationToken cancellationToken = default)
    {
        return await _context.Pessoas.FirstOrDefaultAsync(p => p.Documento == documento, cancellationToken);
    }

    public async Task<IEnumerable<Pessoa>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Pessoas.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Pessoa>> ObterAtivosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Pessoas.Where(p => p.EstaAtivo).ToListAsync(cancellationToken);
    }

    public async Task<Pessoa> AdicionarAsync(Pessoa pessoa, CancellationToken cancellationToken = default)
    {
        await _context.Pessoas.AddAsync(pessoa, cancellationToken);
        return pessoa;
    }

    public Task AtualizarAsync(Pessoa pessoa, CancellationToken cancellationToken = default)
    {
        _context.Pessoas.Update(pessoa);
        return Task.CompletedTask;
    }

    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pessoa = await ObterPorIdAsync(id, cancellationToken);
        if (pessoa != null)
        {
            _context.Pessoas.Remove(pessoa);
        }
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Pessoas.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExisteAsync(string email, Guid? excluirId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Pessoas.Where(p => p.Email == email);

        if (excluirId.HasValue)
        {
            query = query.Where(p => p.Id != excluirId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> DocumentoExisteAsync(string documento, Guid? excluirId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Pessoas.Where(p => p.Documento == documento);

        if (excluirId.HasValue)
        {
            query = query.Where(p => p.Id != excluirId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}