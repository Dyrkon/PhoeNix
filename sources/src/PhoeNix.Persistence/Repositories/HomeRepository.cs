using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Repositories;

namespace PhoeNix.Persistence.Repositories;

internal sealed class HomeRepository : Repository<Home, HomeId>, IHomeRepository
{
    public HomeRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Home?> GetByNameAsync(string name, CancellationToken token)
    {
        return DbContext.Homes.SingleOrDefaultAsync(h => h.Name.Contains(name), cancellationToken: token);
    }
}