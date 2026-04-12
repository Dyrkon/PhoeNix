using Microsoft.EntityFrameworkCore.Storage;

namespace PhoeNix.Application.Repositories;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken token);
    Task SaveChangesAsync(CancellationToken token);
}