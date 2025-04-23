using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Repositories;

namespace PhoeNix.Persistence.Repositories;

internal sealed class HomeRepository : RepositoryBase<Home, HomeId>, IHomeRepository
{
    public HomeRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public override Task<Home?> GetByIdAsync(HomeId id, CancellationToken token)
    {
        return DbContext.Homes
            .Include(home => home.Modules)
            .ThenInclude(homeModule => homeModule.Module)
            .Include(home => home.Users)
            .ThenInclude(homeUser => homeUser.User)
            .AsSplitQuery()
            .SingleOrDefaultAsync(home => home.Id == id, cancellationToken: token);
    }

    public Task<Home?> GetByNameAsync(string name, CancellationToken token)
    {
        return DbContext.Homes
            .Include(home => home.Modules)
            .ThenInclude(homeModule => homeModule.Module)
            .Include(home => home.Users)
            .ThenInclude(homeUser => homeUser.User)
            .AsSplitQuery()
            .SingleOrDefaultAsync(home => home.Name.Contains(name), token);
    }
}