using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Domain.Services;
using PhoeNix.Infrastructure.Services;

namespace PhoeNix.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<INixFormatterService, NixFormatterService>();
        services.AddSingleton<IModuleFilesBuilder, ModuleFilesBuilder>();
        services.AddSingleton<IConfigurationFilesBuilder, ConfigurationFilesBuilder>();
        services.AddSingleton<INixTestRunner, NixTestRunner>();
        services.AddSingleton<INixErrorParserService, NixErrorParserService>();
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddScoped<INixBuildMaterializer, NixBuildMaterializer>();

        return services;
    }
}