using Carter;
using Microsoft.Extensions.DependencyInjection;

namespace PhoeNix.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddCarter();
        return services;
    }
}