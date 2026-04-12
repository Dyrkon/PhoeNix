using Microsoft.EntityFrameworkCore.Storage;
using PhoeNix.Application.Repositories;

namespace PhoeNix.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken token)
    {
        var existing = _context.Database.CurrentTransaction;
        if (existing is not null)
            return Task.FromResult<IDbContextTransaction>(new NoOpDbContextTransaction(existing));

        return _context.Database.BeginTransactionAsync(token);
    }

    public Task SaveChangesAsync(CancellationToken token)
    {
        return _context.SaveChangesAsync(token);
    }
}

file sealed class NoOpDbContextTransaction(IDbContextTransaction inner) : IDbContextTransaction
{
    public Guid TransactionId => inner.TransactionId;

    public void Commit()
    {
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Rollback()
    {
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}