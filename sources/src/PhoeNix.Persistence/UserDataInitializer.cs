using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions;
using PhoeNix.Application.Options;
using PhoeNix.Domain.Entities.AppSettings;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Persistence.Seeding;

namespace PhoeNix.Persistence;

internal sealed class UserDataInitializer(
    ApplicationDbContext dbContext,
    IOptions<SeedExampleOptions> seedOptions) : IUserDataInitializer
{
    public async Task InitializeForUserAsync(UserId userId, CancellationToken cancellationToken)
    {
        var appSettings = AppSettings.CreateDefault(new AppSettingsId(Guid.NewGuid()), userId);
        dbContext.AppSettings.Add(appSettings);

        var templateResult = ModuleTemplateSeedFactory.CreateAll(userId);
        if (templateResult.IsFailure)
            throw new InvalidOperationException(templateResult.Error.Description);

        dbContext.ModuleTemplates.AddRange(templateResult.Value.Templates);

        var templateIds = templateResult.Value.ById;
        var options = seedOptions.Value;

        var configurations = new[]
        {
            ConfigurationSeedFactory.CreateMinimalInstallableExample(userId, templateIds, options),
            ConfigurationSeedFactory.CreatePhoeNixDeploymentExample(userId, templateIds, options),
            ConfigurationSeedFactory.CreateCacheMachineExample(userId, templateIds, options),
            ConfigurationSeedFactory.CreateGnomeWorkstationExample(userId, templateIds, options),
            ConfigurationSeedFactory.CreateKdeWorkstationExample(userId, templateIds, options),
        };

        foreach (var result in configurations)
        {
            if (result.IsFailure)
                throw new InvalidOperationException(result.Error.Description);

            dbContext.Configurations.Add(result.Value);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
