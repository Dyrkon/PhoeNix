using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Modules;

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

    public async Task<IReadOnlyList<ModuleTemplate>> GetByIdsAsync(
        IReadOnlyCollection<ModuleTemplateId> ids,
        CancellationToken token)
    {
        if (ids.Count == 0)
            return [];

        return await DbContext.ModuleTemplates
            .Include(m => m.EditableValueTypes)
            .Include(m => m.Tests)
            .Where(m => ids.Contains(m.Id))
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