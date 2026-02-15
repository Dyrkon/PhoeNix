using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;
using PhoeNix.Persistence.Extensions;

namespace PhoeNix.Persistence.Repositories;

internal sealed class ConfigurationRepository : RepositoryBase<Configuration, ConfigurationId>,
    IConfigurationRepository
{
    public ConfigurationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Configuration?> GetByDescriptionAsync(string description, CancellationToken token)
    {
        return await DbContext.Configurations
            .AddIncludeStatements()
            .SingleOrDefaultAsync(c => c.Description.Contains(description), token);
    }

    public async Task<Configuration?> GetByTitleAsync(string title, CancellationToken token)
    {
        return await DbContext.Configurations
            .AddIncludeStatements()
            .SingleOrDefaultAsync(c => c.Title.Contains(title), token);
    }

    public async Task<Result> RemoveByIdAsync(ConfigurationId id, CancellationToken token)
    {
        var tmp = await DbContext.Configurations.AddIncludeStatements()
            .SingleOrDefaultAsync(c => c.Id == id, token);
        if (tmp == null)
            return Result.Failure(new Error("", $"Configuration with id {id.Value} was not found"));
        DbContext.Configurations.Remove(tmp);
        return Result.Success();
    }

    public override async Task<Configuration?> GetByIdAsync(ConfigurationId id, CancellationToken token)
    {
        return await DbContext.Configurations
            .AddIncludeStatements()
            .SingleOrDefaultAsync(c => c.Id == id, token);
    }
}