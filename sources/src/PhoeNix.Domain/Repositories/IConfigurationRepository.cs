using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Repositories;

public interface IConfigurationRepository : IRepository<Configuration, ConfigurationId>
{
    Task<Configuration?> GetByDescriptionAsync(string description, CancellationToken token);
    Task<Configuration?> GetByTitleAsync(string title, CancellationToken token);
    Task<Result> RemoveByIdAsync(ConfigurationId id, CancellationToken token);
}