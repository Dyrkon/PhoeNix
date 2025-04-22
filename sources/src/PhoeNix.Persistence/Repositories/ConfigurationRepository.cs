using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Repositories;
using PhoeNix.Persistence.Extensions;

namespace PhoeNix.Persistence.Repositories;

internal sealed class ConfigurationRepository : RepositoryBase<Configuration, ConfigurationId>,
    IConfigurationRepository
{
    public ConfigurationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Configuration?> GetByDescriptionAsync(string description, CancellationToken token)
    {
        return DbContext.Configurations
            .AddIncludeStatements()
            .SingleOrDefaultAsync(c => c.Description.Contains(description), token);
    }

    public Task<Configuration?> GetByTitleAsync(string title, CancellationToken token)
    {
        return DbContext.Configurations
            .AddIncludeStatements()
            .SingleOrDefaultAsync(c => c.Title.Contains(title), token);
    }

    public override Task<Configuration?> GetByIdAsync(ConfigurationId id, CancellationToken token)
    {
        return DbContext.Configurations
            .AddIncludeStatements()
            .SingleOrDefaultAsync(c => c.Id == id, token);
    }
}