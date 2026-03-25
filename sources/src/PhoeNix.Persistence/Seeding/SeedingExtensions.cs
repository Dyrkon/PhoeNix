using Microsoft.Extensions.DependencyInjection;

namespace PhoeNix.Persistence.Seeding;

public static class SeedingExtensions
{
    public static async Task SeedApplicationDataAsync(this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<ApplicationDbSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}