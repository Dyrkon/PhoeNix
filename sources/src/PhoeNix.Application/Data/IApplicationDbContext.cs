using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Entities.SystemUsers;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Application.Data;

public interface IApplicationDbContext
{
    public DbSet<Configuration> Configurations { get; }
    public DbSet<Domain.Entities.Systems.System> Systems { get; }
    public DbSet<SystemUser> SystemUsers { get; }
    public DbSet<ModuleTemplate> ModuleTemplates { get; }
    public DbSet<Test> Tests { get; }
    public DbSet<ModuleValue> ModuleValue { get; }
    public DbSet<EntryValue> EntryValues { get; }
    public DbSet<Input> Inputs { get; }
    public DbSet<User> Users { get; }
    public DbSet<ProvisioningSession> ProvisioningSessions { get; }
}