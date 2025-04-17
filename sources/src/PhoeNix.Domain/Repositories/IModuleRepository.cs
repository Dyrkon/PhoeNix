using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Domain.Repositories;

public interface IModuleRepository : IRepository<Module, ModuleId>
{
    Task<Module?> GetByNameAsync(string name, CancellationToken token);
}