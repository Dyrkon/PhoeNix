using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Persistence.Repositories;

internal sealed class ModuleTemplateRepository : RepositoryBase<ModuleTemplate, ModuleTemplateId>,
    IModuleTemplateRepository
{
    public ModuleTemplateRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<ModuleTemplate?> GetByNameAsync(string name, UserId ownerId, CancellationToken token)
    {
        return await DbContext.ModuleTemplates
            .Include(m => m.EditableValueTypes)
            .Include(m => m.Tests)
            .SingleOrDefaultAsync(m => m.Name == name && m.OwnerId == ownerId, token);
    }

    public async Task<IEnumerable<ModuleTemplate>> GetAllAsync(UserId ownerId, CancellationToken token)
    {
        return await DbContext.ModuleTemplates
            .Include(m => m.EditableValueTypes)
            .Include(m => m.Tests)
            .Where(m => m.OwnerId == ownerId)
            .OrderBy(m => m.Name)
            .ToListAsync(token);
    }

    public async Task<IReadOnlyList<ModuleTemplate>> GetByIdsAsync(
        IReadOnlyCollection<ModuleTemplateId> ids,
        UserId ownerId,
        CancellationToken token)
    {
        if (ids.Count == 0)
            return [];

        return await DbContext.ModuleTemplates
            .Include(m => m.EditableValueTypes)
            .Include(m => m.Tests)
            .Where(m => ids.Contains(m.Id) && m.OwnerId == ownerId)
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