using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Application.Data;
using PhoeNix.Domain.Repositories;
using PhoeNix.Persistence.Repositories;

namespace PhoeNix.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("ConnectionStrings");
            options.UseSqlite(connectionString);
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        services.AddScoped<IHomeRepository, HomeRepository>();
        services.AddScoped<IInputRepository, InputRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<ISystemRepository, SystemRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}