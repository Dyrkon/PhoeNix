using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhoeNix.Application.Options;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Persistence.Seeding;

internal sealed class ApplicationDbSeeder(
    ApplicationDbContext dbContext,
    IOptions<SeedExampleOptions> seedExampleOptions)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        var templatesExist = await dbContext.Set<ModuleTemplate>()
            .AnyAsync(
                t => t.Id == SeedIds.MinimalBaseTemplate
                     || t.Id == SeedIds.DiskoEfiExt4Template
                     || t.Id == SeedIds.CallbackTemplate
                     || t.Id == SeedIds.PrometheusTemplate,
                cancellationToken);

        if (!templatesExist)
        {
            var templatesResult = ModuleTemplateSeedFactory.CreateAll();
            if (templatesResult.IsFailure)
                throw new InvalidOperationException(templatesResult.Error.Description);

            dbContext.Set<ModuleTemplate>().AddRange(templatesResult.Value);
        }

        var configurationExists = await dbContext.Set<Configuration>()
            .AnyAsync(c => c.Id == SeedIds.ExampleConfiguration, cancellationToken);

        if (!configurationExists)
        {
            var configurationResult =
                ConfigurationSeedFactory.CreateMinimalInstallableExample(seedExampleOptions.Value);
            if (configurationResult.IsFailure)
                throw new InvalidOperationException(configurationResult.Error.Description);

            dbContext.Set<Configuration>().Add(configurationResult.Value);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}