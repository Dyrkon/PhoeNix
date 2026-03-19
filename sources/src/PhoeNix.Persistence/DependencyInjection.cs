using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Application.Data;
using PhoeNix.Domain.Repositories;
using PhoeNix.Persistence.Repositories;
using PhoeNix.Persistence.Seeding;

namespace PhoeNix.Persistence;

public static class DependencyInjection
{
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        services.AddScoped<IModuleTemplateRepository, ModuleTemplateRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMachineRepository, MachineRepository>();
        services.AddScoped<ISetupSessionRepository, SetupSessionRepository>();

        return services;
    }

    private static IServiceCollection ConfigureDbContext(this IServiceCollection services)
    {
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                // TODO the DB shouldn't be in TMP folder
                var dbName = Path.Combine(Path.GetTempPath(),
                    configuration.GetConnectionString("PhoeNix") ?? "phoenix.db");
                options.UseSqlite($"Data Source={dbName}");
            })
            .ConfigureDbContext()
            .AddRepositories();

        return services;
    }

    public static IServiceCollection AddInMemoryPersistence(this IServiceCollection services, string dbName = "TestDb")
    {
        services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(dbName))
            .ConfigureDbContext()
            .AddRepositories();

        return services;
    }

    public static IServiceCollection AddSeeding(this IServiceCollection services)
    {
        services.AddScoped<ApplicationDbSeeder>();

        return services;
    }
}