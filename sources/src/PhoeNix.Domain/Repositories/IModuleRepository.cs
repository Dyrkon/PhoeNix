using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Domain.Repositories;

public interface IModuleRepository : IRepository<ModuleTemplate, ModuleTemplateId>
{
    Task<ModuleTemplate?> GetByNameAsync(string name, CancellationToken token);
}