namespace PhoeNix.Domain.Repositories;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken token);
}