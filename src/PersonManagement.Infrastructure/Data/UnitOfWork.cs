using Microsoft.EntityFrameworkCore.Storage;
using PersonManagement.Domain.Interfaces;
using PersonManagement.Infrastructure.Repositories;

namespace PersonManagement.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    private IPessoaRepository? _pessoaRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IPessoaRepository PessoaRepository =>
        _pessoaRepository ??= new PessoaRepository(_context);

    public async Task<int> SalvarMudancasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task IniciarTransacaoAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task ConfirmarTransacaoAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task ReverterTransacaoAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}