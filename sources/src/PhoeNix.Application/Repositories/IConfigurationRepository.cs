using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Repositories;

public interface IConfigurationRepository : IRepository<Configuration, ConfigurationId>
{
    Task<IEnumerable<Configuration>> GetAllAsync(CancellationToken token);
    Task<Configuration?> GetByDescriptionAsync(string description, CancellationToken token);
    Task<Configuration?> GetByTitleAsync(string title, CancellationToken token);
    Task<Result> RemoveByIdAsync(ConfigurationId id, CancellationToken token);
    Task<IReadOnlyList<Configuration>> GetAllUsingModuleTemplateAsync(ModuleTemplateId moduleTemplateId, CancellationToken token);
}