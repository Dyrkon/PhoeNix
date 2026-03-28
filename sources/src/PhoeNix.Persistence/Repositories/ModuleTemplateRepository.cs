using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Repositories;

namespace PhoeNix.Persistence.Repositories;

internal sealed class ModuleTemplateRepository : RepositoryBase<ModuleTemplate, ModuleTemplateId>,
    IModuleTemplateRepository
{
    public ModuleTemplateRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<ModuleTemplate?> GetByNameAsync(string name, CancellationToken token)
    {
        return await DbContext.ModuleTemplates
            .Include(m => m.EditableValueTypes)
            .Include(m => m.Tests)
            .SingleOrDefaultAsync(m => m.Name == name, token);
    }

    public async Task<IEnumerable<ModuleTemplate>> GetAllAsync(CancellationToken token)
    {
        return await DbContext.ModuleTemplates
            .Include(m => m.EditableValueTypes)
            .Include(m => m.Tests)
            .OrderBy(m => m.Name)
            .ToListAsync(token);
    }

    public override Task<ModuleTemplate?> GetByIdAsync(ModuleTemplateId templateId, CancellationToken token)
    {
        return DbContext.ModuleTemplates
            .Include(m => m.EditableValueTypes)
            .Include(m => m.Tests)
            .SingleOrDefaultAsync(c => c.Id == templateId, token);
    }
}