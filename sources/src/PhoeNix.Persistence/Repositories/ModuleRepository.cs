using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Repositories;

namespace PhoeNix.Persistence.Repositories;

internal sealed class ModuleRepository : RepositoryBase<Module, ModuleId>, IModuleRepository
{
    public ModuleRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Module?> GetByNameAsync(string name, CancellationToken token)
    {
        return DbContext.Modules
            .Include(m => m.Entries)
            .SingleOrDefaultAsync(m => m.Name.Contains(name), token);
    }

    public override Task<Module?> GetByIdAsync(ModuleId id, CancellationToken token)
    {
        return DbContext.Modules
            .Include(m => m.Entries)
            .SingleOrDefaultAsync(m => m.Id == id, token);
    }
}