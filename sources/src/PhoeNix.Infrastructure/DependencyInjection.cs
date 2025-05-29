using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Domain.Services;
using PhoeNix.Infrastructure.Services;

namespace PhoeNix.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<INixFormatterService, NixFormatterService>();
        services.AddSingleton<IModuleBuilderService, ModuleBuilderService>();
        services.AddSingleton<IConfigurationBuilderService, ConfigurationBuilderService>();

        return services;
    }
}