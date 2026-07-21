using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Data;
using PhoeNix.Application.Models.Outbox;
using PhoeNix.Domain.Entities.AppSettings;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Entities.VmHosts;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence;

using Outbox;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Configuration> Configurations { get; set; }
    public DbSet<Input> Inputs { get; set; }
    public DbSet<ModuleTemplate> ModuleTemplates { get; set; }
    public DbSet<Test> Tests { get; set; }
    public DbSet<EntryValue> EntryValues { get; set; }
    public DbSet<Domain.Entities.Systems.System> Systems { get; set; }
    public DbSet<ModuleValue> ModuleValue { get; set; }
    public DbSet<Machine> Machines { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<SetupSession> SetupSessions { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<AppSettings> AppSettings { get; set; }
    public DbSet<VmHost> VmHosts { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly,
            type =>
                !type.IsAbstract &&
                !type.IsGenericTypeDefinition &&
                type.IsAssignableTo(typeof(IApplicationEntityTypeConfiguration)));

        base.OnModelCreating(modelBuilder);
    }
}