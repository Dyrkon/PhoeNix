using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Data;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Persistence.Configurations.Abstractions;
using Module = PhoeNix.Domain.Entities.Modules.Module;

namespace PhoeNix.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Configuration> Configurations { get; set; }
    public DbSet<Home> Homes { get; set; }
    public DbSet<Input> Inputs { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<Domain.Entities.Systems.System> Systems { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ConfigurationHome> ConfigurationHomes { get; set; }
    public DbSet<ConfigurationSystem> ConfigurationSystems { get; set; }
    public DbSet<ConfigurationModule> ConfigurationModules { get; set; }
    public DbSet<ConfigurationInput> ConfigurationInput { get; set; }
    public DbSet<HomeModule> HomeModules { get; set; }
    public DbSet<HomeUser> HomeUsers { get; set; }
    public DbSet<SystemModule> SystemModules { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly,
            type =>
            {
                var result = type.IsAssignableTo(typeof(IApplicationEntityTypeConfiguration));
                return result;
            }
        );

        SeedDb(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private void SeedDb(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasData(
                User.Create(
                    new UserId(Guid.NewGuid())
                ).Value
            );
    }
}