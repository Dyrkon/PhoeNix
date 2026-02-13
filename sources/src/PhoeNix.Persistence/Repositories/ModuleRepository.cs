using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Repositories;

namespace PhoeNix.Persistence.Repositories;

internal sealed class ModuleRepository : RepositoryBase<ModuleTemplate, ModuleTemplateId>, IModuleRepository
{
    public ModuleRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<ModuleTemplate?> GetByNameAsync(string name, CancellationToken token)
    {
        return DbContext.Modules
            .Include(m => m.EditableValueTypes)
            .Include(m => m.Tests)
            .ThenInclude(m => m.VariableNames)
            .SingleOrDefaultAsync(m => m.Name.Contains(name), token);
    }

    public override Task<ModuleTemplate?> GetByIdAsync(ModuleTemplateId templateId, CancellationToken token)
    {
        return DbContext.Modules
            .Include(m => m.EditableValueTypes)
            .Include(m => m.Tests)
            .ThenInclude(m => m.VariableNames)
            .SingleOrDefaultAsync(c => c.Id == templateId, token);
    }
}