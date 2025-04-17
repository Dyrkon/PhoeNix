using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Repositories;

namespace PhoeNix.Persistence.Repositories;

internal sealed class ConfigurationRepository : Repository<Configuration, ConfigurationId>, IConfigurationRepository
{
    public ConfigurationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Configuration?> GetByDescriptionAsync(string description, CancellationToken token)
    {
        return DbContext.Configurations.SingleOrDefaultAsync(c => c.Description.Contains(description), cancellationToken: token);
    }

    public Task<Configuration?> GetByTitleAsync(string title, CancellationToken token)
    {
        return DbContext.Configurations.SingleOrDefaultAsync(c => c.Title.Contains(title), cancellationToken: token);
    }
}
