using Microsoft.EntityFrameworkCore;

namespace PhoeNix.Persistence.Seeding;

internal sealed class ApplicationDbSeeder(ApplicationDbContext dbContext)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
