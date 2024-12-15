using Domain.Entities.Machine;
using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Data;
using PhoeNix.Persistence.Configuration.Abstractions;

namespace PhoeNix.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public System.Data.Entity.DbSet<Machine> Machines { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly,
            type => type.IsAssignableTo(typeof(IApplicationDbContextEntityTypeConfiguration)));

        SeedDb(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private void SeedDb(ModelBuilder modelBuilder)
    {
        // TODO
    }
}