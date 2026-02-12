using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Repositories;

namespace PhoeNix.Persistence.Repositories;

internal sealed class ModuleRepository : RepositoryBase<ModuleTemplate, ModuleId>, IModuleRepository
{
    public ModuleRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<ModuleTemplate?> GetByNameAsync(string name, CancellationToken token)
    {
        return DbContext.Modules
            .Include(m => m.EditableValues)
            .Include(m => m.Tests)
            .ThenInclude(m => m.Test)
            .SingleOrDefaultAsync(m => m.Name.Contains(name), token);
    }

    public override Task<ModuleTemplate?> GetByIdAsync(ModuleId id, CancellationToken token)
    {
        return DbContext.Modules
            .Include(m => m.EditableValues)
            .Include(m => m.Tests)
            .ThenInclude(m => m.Test)
            .SingleOrDefaultAsync(c => c.Id == id, token);
    }
}