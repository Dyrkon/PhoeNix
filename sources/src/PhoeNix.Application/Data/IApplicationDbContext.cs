using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Models.Outbox;
using PhoeNix.Domain.Entities.AppSettings;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Application.Data;

public interface IApplicationDbContext
{
    public DbSet<Configuration> Configurations { get; }
    public DbSet<Domain.Entities.Systems.System> Systems { get; }
    public DbSet<ModuleTemplate> ModuleTemplates { get; }
    public DbSet<Test> Tests { get; }
    public DbSet<ModuleValue> ModuleValue { get; }
    public DbSet<EntryValue> EntryValues { get; }
    public DbSet<Input> Inputs { get; }
    public DbSet<User> Users { get; }
    public DbSet<SetupSession> SetupSessions { get; }
    public DbSet<OutboxMessage> OutboxMessages { get; }
    public DbSet<AppSettings> AppSettings { get; }
}