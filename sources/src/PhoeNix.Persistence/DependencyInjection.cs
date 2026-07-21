using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Application.Abstractions;
using PhoeNix.Application.Abstractions.Outbox;
using PhoeNix.Application.Data;
using PhoeNix.Application.Options;
using PhoeNix.Application.Repositories;
using PhoeNix.Persistence.Interceptors;
using PhoeNix.Persistence.Outbox;
using PhoeNix.Persistence.Repositories;
using PhoeNix.Persistence.Seeding;

namespace PhoeNix.Persistence;

public static class DependencyInjection
{
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
        services.AddScoped<IConfigurationReadRepository, ConfigurationReadRepository>();
        services.AddScoped<IModuleTemplateRepository, ModuleTemplateRepository>();
        services.AddScoped<IModuleTemplateReadRepository, ModuleTemplateReadRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMachineRepository, MachineRepository>();
        services.AddScoped<IMachineReadRepository, MachineReadRepository>();
        services.AddScoped<IPrometheusTargetsRepository, PrometheusTargetsRepository>();
        services.AddScoped<ISetupSessionRepository, SetupSessionRepository>();
        services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();
        services.AddScoped<IVmHostRepository, VmHostRepository>();
        services.AddScoped<IVmHostReadRepository, VmHostReadRepository>();

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
        services.AddSingleton(new JsonSerializerOptions(JsonSerializerDefaults.Web));
        services.AddScoped<IOutboxMessageSerializer, OutboxMessageSerializer>();
        services.AddScoped<InsertOutboxMessagesInterceptor>();

        services.Configure<OutboxOptions>(options =>
        {
            options.BatchSize = 20;
            options.PollInterval = TimeSpan.FromSeconds(2);
            options.MaxDegreeOfParallelism = 4;
        });

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var connectionString = configuration.GetConnectionString("PhoeNix")
                                       ?? throw new InvalidOperationException(
                                           "Connection string 'DefaultConnection' is not configured.");
                options.UseNpgsql(connectionString);
                options.AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>());
            })
            .ConfigureDbContext()
            .AddRepositories();

        services.AddHostedService<OutboxProcessorBackgroundService>();

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
        services.AddScoped<IUserDataInitializer, UserDataInitializer>();

        return services;
    }
}