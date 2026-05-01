using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Application.Repositories;

public interface IModuleTemplateRepository : IRepository<ModuleTemplate, ModuleTemplateId>
{
    Task<ModuleTemplate?> GetByNameAsync(string name, UserId ownerId, CancellationToken token);

    Task<IEnumerable<ModuleTemplate>> GetAllAsync(UserId ownerId, CancellationToken token);

    Task<IReadOnlyList<ModuleTemplate>> GetByIdsAsync(
        IReadOnlyCollection<ModuleTemplateId> ids,
        UserId ownerId,
        CancellationToken token);
}