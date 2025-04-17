using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Repositories;

namespace PhoeNix.Persistence.Repositories;

internal sealed class SystemRepository : Repository<Domain.Entities.Systems.System, SystemId>, ISystemRepository
{
    public SystemRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Domain.Entities.Systems.System?> GetByNameAsync(string name, CancellationToken token)
    {
        return DbContext.Systems.SingleOrDefaultAsync(s => s.Name.Contains(name), cancellationToken: token);
    }
}