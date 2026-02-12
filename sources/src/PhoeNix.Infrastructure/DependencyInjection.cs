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
        services.AddSingleton<IModuleRenderer, ModuleRenderer>();
        services.AddSingleton<IConfigurationExportService, ConfigurationExportService>();
        services.AddSingleton<INixTestRunner, NixTestRunner>();
        services.AddSingleton<INixErrorParserService, NixErrorParserService>();
        services.AddSingleton<IProcessRunner, ProcessRunner>();

        return services;
    }
}