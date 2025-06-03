using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Repositories;

namespace PhoeNix.Persistence.Repositories;

internal sealed class SystemRepository : RepositoryBase<Domain.Entities.Systems.System, SystemId>, ISystemRepository
{
    public SystemRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Domain.Entities.Systems.System?> GetByNameAsync(string name, CancellationToken token)
    {
        return DbContext.Systems
            .Include(s => s.Modules)
            .ThenInclude(m => m.Module)
            .ThenInclude(m => m.EditableValues)
            .SingleOrDefaultAsync(s => s.Name.Contains(name), token);
    }

    public override Task<Domain.Entities.Systems.System?> GetByIdAsync(SystemId id, CancellationToken token)
    {
        return DbContext.Systems
            .Include(s => s.Modules)
            .ThenInclude(m => m.Module)
            .ThenInclude(m => m.EditableValues)
            .SingleOrDefaultAsync(s => s.Id == id, token);
    }
}