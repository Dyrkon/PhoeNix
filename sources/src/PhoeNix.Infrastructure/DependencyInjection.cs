using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Bootstrap;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Domain.Services;
using PhoeNix.Infrastructure.Services;
using PhoeNix.Domain.Options;

namespace PhoeNix.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<BootstrapArtifactsOptions>();
        services.AddOptions<NetbootHostOptions>();

        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<INixFormatterService, NixFormatterService>();
        services.AddSingleton<IModuleFilesBuilder, ModuleFilesBuilder>();
        services.AddSingleton<IConfigurationFilesBuilder, ConfigurationFilesBuilder>();
        services.AddSingleton<INixTestRunner, NixTestRunner>();
        services.AddSingleton<INixErrorParserService, NixErrorParserService>();
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddScoped<INixBuildMaterializer, NixBuildMaterializer>();
        services.AddScoped<ISshKeyFileStore, SshKeyFileStore>();
        services.AddScoped<ISshKeyProvider, SshKeyProvider>();
        services.AddSingleton<ICallbackTokenService, JwtCallbackTokenService>();
        services.AddScoped<IBootArtifactBuilder, BootstrapArtifactBuilder>();
        services.AddSingleton<INetbootHostService, NetbootHostService>();

        return services;
    }
}
