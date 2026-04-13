using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Monitoring;
using PhoeNix.Application.Abstractions.Bootstrap;
using PhoeNix.Application.Abstractions.Deployment;
using PhoeNix.Application.Abstractions.FileSystem;
using PhoeNix.Application.Abstractions.HardwareProbing;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Abstractions.Outbox;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Abstractions.Setup;
using PhoeNix.Application.Options;
using PhoeNix.Infrastructure.Services;
using PhoeNix.Infrastructure.Services.Authentication;
using PhoeNix.Infrastructure.Services.Monitoring;
using PhoeNix.Infrastructure.Services.ConfigurationManagement;
using PhoeNix.Infrastructure.Services.Filesystem;
using PhoeNix.Infrastructure.Services.HardwareManagement;
using PhoeNix.Infrastructure.Services.Processes;
using PhoeNix.Infrastructure.Services.Setup;
using PhoeNix.Infrastructure.Services.UtilityWrappers;

namespace PhoeNix.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<NetbootHostOptions>();

        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<INixFormatterService, NixFormatterService>();
        services.AddSingleton<IModuleFilesBuilder, ModuleFilesBuilder>();
        services.AddSingleton<IConfigurationFilesBuilder, ConfigurationFilesBuilder>();
        services.AddSingleton<INixTestRunner, NixTestRunner>();
        services.AddSingleton<INixErrorParserService, NixErrorParserService>();
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<ICallbackTokenService, JwtCallbackTokenService>();
        services.AddSingleton<IPrometheusTokenService, PrometheusTokenService>();
        services.AddHttpClient<IPrometheusQueryClient, PrometheusQueryClient>();
        services.AddSingleton<INetbootHostService, NetbootHostService>();
        services.AddSingleton<INixBuildMaterializer, NixBuildMaterializer>();
        services.AddScoped<ISshKeyFileStore, SshKeyFileStore>();
        services.AddScoped<ISetupSshKeyProvider, SetupSshKeyProvider>();
        services.AddScoped<IBootstrapImageBuilder, BootstrapImageBuilder>();
        services.AddScoped<IInstallDiskSelectionPolicy, InstallDiskSelectionPolicy>();
        services.AddScoped<IHardwareProbeService, SshHardwareProbeService>();
        services.AddScoped<IHardwareInventoryProjector, HardwareInventoryProjector>();
        services.AddScoped<INixosInstaller, NixosAnywhereInstaller>();
        services.AddScoped<IRuntimeBindingResolver, RuntimeBindingResolver>();
        services.AddScoped<ISetupWorkflowDecider, SetupWorkflowDecider>();
        services.AddScoped<ISetupSshKeyProvider, SetupSshKeyProvider>();
        services.AddScoped<IDeploySshKeyProvider, DeploySshKeyProvider>();
        services.AddScoped<INixOsMachineUpdater, NixOsMachineUpdater>();
        services.AddScoped<IDeploymentBindingResolver, DeploymentBindingResolver>();
        services.AddScoped<IUserPasswordHasher, AspNetUserPasswordHasher>();
        services.AddScoped<IUserSessionService, CookieUserSessionService>();
        services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
        services.AddHostedService<PrometheusTokenWriterService>();

        return services;
    }
}