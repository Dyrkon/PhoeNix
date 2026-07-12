using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Shared;
using PhoeNix.Persistence.Extensions;

namespace PhoeNix.Persistence.Repositories;

internal sealed class ConfigurationRepository : RepositoryBase<Configuration, ConfigurationId>,
    IConfigurationRepository
{
    public ConfigurationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<Configuration>> GetAllAsync(CancellationToken token)
    {
        return await DbContext.Configurations
            .OrderBy(c => c.Title)
            .ToListAsync(token);
    }

    public async Task<Configuration?> GetByDescriptionAsync(string description, CancellationToken token)
    {
        return await DbContext.Configurations
            .SingleOrDefaultAsync(c => c.Description == description, token);
    }

    public async Task<Configuration?> GetByTitleAsync(string title, CancellationToken token)
    {
        return await DbContext.Configurations
            .SingleOrDefaultAsync(c => c.Title == title, token);
    }

    public async Task<Configuration?> GetByTitleAsync(string title, UserId ownerId, CancellationToken token)
    {
        return await DbContext.Configurations
            .SingleOrDefaultAsync(c => c.Title == title && c.OwnerId == ownerId, token);
    }

    public async Task<Result> RemoveByIdAsync(ConfigurationId id, CancellationToken token)
    {
        var configuration = await DbContext.Configurations
            .AddIncludeStatements()
            .SingleOrDefaultAsync(c => c.Id == id, token);

        if (configuration is null)
            return Result.Failure(new Error("Configurations.NotFound", $"Configuration '{id.Value}' was not found."));

        DbContext.Configurations.Remove(configuration);
        return Result.Success();
    }

    public override async Task<Configuration?> GetByIdAsync(ConfigurationId id, CancellationToken token)
    {
        return await DbContext.Configurations
            .AddIncludeStatements()
            .SingleOrDefaultAsync(c => c.Id == id, token);
    }
}