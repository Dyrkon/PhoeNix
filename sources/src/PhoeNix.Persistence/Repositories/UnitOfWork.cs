using PhoeNix.Application.Repositories;

namespace PhoeNix.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task SaveChangesAsync(CancellationToken token)
    {
        return _context.SaveChangesAsync(token);
    }
}