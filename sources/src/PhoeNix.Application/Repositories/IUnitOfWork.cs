namespace PhoeNix.Application.Repositories;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken token);
}