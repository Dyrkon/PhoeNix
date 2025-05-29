using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Application.Data;
using PhoeNix.Domain.Repositories;
using PhoeNix.Persistence.Repositories;

namespace PhoeNix.Persistence;

public static class DependencyInjection
{
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        services.AddScoped<IHomeRepository, HomeRepository>();
        services.AddScoped<IInputRepository, InputRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<ISystemRepository, SystemRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

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
}