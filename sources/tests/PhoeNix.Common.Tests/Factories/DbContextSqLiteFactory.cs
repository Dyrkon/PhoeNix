using Microsoft.EntityFrameworkCore;

namespace PhoeNix.Persistence.Tests;

public class DbContextSqLiteFactory : IDbContextFactory<ApplicationDbContext>
{
    private readonly DbContextOptionsBuilder<ApplicationDbContext> _contextOptionsBuilder = new();

    public DbContextSqLiteFactory(string databaseName)
    {
        _contextOptionsBuilder.UseSqlite("Data Source =:memory:;");

        _contextOptionsBuilder.EnableSensitiveDataLogging();
    }

    public ApplicationDbContext CreateDbContext()
    {
        return new ApplicationDbContext(_contextOptionsBuilder.Options);
    }
}