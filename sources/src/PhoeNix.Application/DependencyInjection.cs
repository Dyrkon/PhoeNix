using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Application.Behaviors;

namespace PhoeNix.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            config.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });

        return services;
    }
}