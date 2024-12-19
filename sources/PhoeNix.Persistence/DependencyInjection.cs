using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PhoeNix.Application.Data;
using PhoeNix.Persistence.Interceptors;
using PhoeNix.Persistence.Options;
using PhoeNix.Persistence.Repositories;

namespace PhoeNix.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ConvertDomainEventsToOutboxMessagesInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var outboxInterceptor = sp.GetService<ConvertDomainEventsToOutboxMessagesInterceptor>();

            options
                .UseNpgsql(configuration.GetNpgsqlConnectionString(),
                    builder => builder.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "phoenix")
                ).AddInterceptors(outboxInterceptor)
                .UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IMachinesRepository, MachinesRepository>();

        return services;
    }

    private static string GetNpgsqlConnectionString(
        this IConfiguration configuration,
        string? databaseName = null)
    {
        var databaseOptions = configuration.GetRequiredSection("Database").Get<DatabaseOptions>()!;

        NpgsqlConnectionStringBuilder npgsqlConnectionStringBuilder = new()
        {
            Host = databaseOptions.Host,
            Port = databaseOptions.Port,
            Database = databaseName ?? databaseOptions.Database,
            Username = databaseOptions.UserName,
            Password = databaseOptions.Password
        };

        return npgsqlConnectionStringBuilder.ConnectionString;
    }
}