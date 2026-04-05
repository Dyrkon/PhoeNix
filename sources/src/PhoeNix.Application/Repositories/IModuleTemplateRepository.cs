using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Application.Repositories;

public interface IModuleTemplateRepository : IRepository<ModuleTemplate, ModuleTemplateId>
{
    Task<ModuleTemplate?> GetByNameAsync(string name, CancellationToken token);

    Task<IEnumerable<ModuleTemplate>> GetAllAsync(CancellationToken token);

    Task<IReadOnlyList<ModuleTemplate>> GetByIdsAsync(
        IReadOnlyCollection<ModuleTemplateId> ids,
        CancellationToken token);
}