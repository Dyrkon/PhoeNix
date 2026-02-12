using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Domain.Repositories;

public interface IModuleRepository : IRepository<ModuleTemplate, ModuleId>
{
    Task<ModuleTemplate?> GetByNameAsync(string name, CancellationToken token);
}