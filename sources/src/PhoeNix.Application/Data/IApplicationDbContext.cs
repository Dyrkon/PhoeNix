using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Application.Data;

public interface IApplicationDbContext
{
    DbSet<Configuration> Configurations { get; }
    DbSet<Domain.Entities.Systems.System> Systems { get; }
    DbSet<User> Users { get; }
    DbSet<ModuleTemplate> ModuleTemplates { get; }
    DbSet<Test> Tests { get; }
    DbSet<ModuleValue> ConfigurationModules { get; }
}