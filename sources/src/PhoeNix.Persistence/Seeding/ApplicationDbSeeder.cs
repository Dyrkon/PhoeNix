using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhoeNix.Application.Options;
using PhoeNix.Domain.Entities.AppSettings;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Persistence.Seeding;

internal sealed class ApplicationDbSeeder(
    ApplicationDbContext dbContext,
    IOptions<SeedExampleOptions> seedExampleOptions,
    IOptions<FileStorageOptions> fileStorageOptions,
    IOptions<SshCaOptions> sshCaOptions,
    IOptions<DeploySshCaOptions> deploySshCaOptions,
    IOptions<HardwareProbeOptions> hardwareProbeOptions,
    IOptions<NixosInstallerOptions> nixosInstallerOptions,
    IOptions<NixOsUpdaterOptions> nixOsUpdaterOptions,
    IOptions<MonitoringOptions> monitoringOptions,
    IOptions<NetbootHostOptions> netbootHostOptions)
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

        var phoenixDeploymentExists = await dbContext.Set<Configuration>()
            .AnyAsync(c => c.Id == SeedIds.PhoeNixDeploymentConfiguration, cancellationToken);

        if (!phoenixDeploymentExists)
        {
            var result = ConfigurationSeedFactory.CreatePhoeNixDeploymentExample(seedExampleOptions.Value);
            if (result.IsFailure)
                throw new InvalidOperationException(result.Error.Description);

            dbContext.Set<Configuration>().Add(result.Value);
        }

        var cacheMachineExists = await dbContext.Set<Configuration>()
            .AnyAsync(c => c.Id == SeedIds.CacheMachineConfiguration, cancellationToken);

        if (!cacheMachineExists)
        {
            var result = ConfigurationSeedFactory.CreateCacheMachineExample(seedExampleOptions.Value);
            if (result.IsFailure)
                throw new InvalidOperationException(result.Error.Description);

            dbContext.Set<Configuration>().Add(result.Value);
        }

        var gnomeWorkstationExists = await dbContext.Set<Configuration>()
            .AnyAsync(c => c.Id == SeedIds.GnomeWorkstationConfiguration, cancellationToken);

        if (!gnomeWorkstationExists)
        {
            var result = ConfigurationSeedFactory.CreateGnomeWorkstationExample(seedExampleOptions.Value);
            if (result.IsFailure)
                throw new InvalidOperationException(result.Error.Description);

            dbContext.Set<Configuration>().Add(result.Value);
        }

        var kdeWorkstationExists = await dbContext.Set<Configuration>()
            .AnyAsync(c => c.Id == SeedIds.KdeWorkstationConfiguration, cancellationToken);

        if (!kdeWorkstationExists)
        {
            var result = ConfigurationSeedFactory.CreateKdeWorkstationExample(seedExampleOptions.Value);
            if (result.IsFailure)
                throw new InvalidOperationException(result.Error.Description);

            dbContext.Set<Configuration>().Add(result.Value);
        }

        var appSettingsExist = await dbContext.AppSettings
            .AnyAsync(cancellationToken);

        if (!appSettingsExist)
        {
            var entity = AppSettings.CreateDefault(SeedIds.DefaultAppSettings);
            entity.Update(
                fileStorageOptions.Value.RootPath,
                sshCaOptions.Value.CaKeyName,
                sshCaOptions.Value.Principal,
                sshCaOptions.Value.CertificateTtl.TotalHours,
                sshCaOptions.Value.KeyType,
                deploySshCaOptions.Value.KeyType,
                deploySshCaOptions.Value.CaKeyName,
                deploySshCaOptions.Value.Principal,
                deploySshCaOptions.Value.DeployUser,
                deploySshCaOptions.Value.CertificateTtl.TotalDays,
                hardwareProbeOptions.Value.SshExecutable,
                hardwareProbeOptions.Value.BootstrapUser,
                hardwareProbeOptions.Value.ProbeCommand,
                hardwareProbeOptions.Value.ConnectTimeoutSeconds,
                hardwareProbeOptions.Value.ProbeTimeoutSeconds,
                hardwareProbeOptions.Value.DisableHostKeyChecking,
                nixosInstallerOptions.Value.ExecutableName,
                nixosInstallerOptions.Value.TargetUser,
                nixosInstallerOptions.Value.InstallTimeoutMinutes,
                nixosInstallerOptions.Value.DisableHostKeyChecking,
                nixosInstallerOptions.Value.BuildOnTarget,
                nixosInstallerOptions.Value.CopyHostKeys,
                nixOsUpdaterOptions.Value.BuildHost,
                nixOsUpdaterOptions.Value.UseRemoteSudo,
                nixOsUpdaterOptions.Value.Fast,
                monitoringOptions.Value.PrometheusEndpoint,
                monitoringOptions.Value.TokenTtl.TotalDays,
                netbootHostOptions.Value.ApiBasePublicUrl,
                netbootHostOptions.Value.HostExecutablePath,
                netbootHostOptions.Value.ListenAddress,
                netbootHostOptions.Value.Port);
            dbContext.AppSettings.Add(entity);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}