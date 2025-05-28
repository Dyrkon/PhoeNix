using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Domain.Service;
using PhoeNix.Infrastructure.Services;

namespace PhoeNix.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystemService, FileSystemService>();

        return services;
    }
}