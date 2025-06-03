using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Application.Data;

public interface IApplicationDbContext
{
    DbSet<Configuration> Configurations { get; }
    DbSet<Home> Homes { get; }
    DbSet<Input> Inputs { get; }
    DbSet<Domain.Entities.Systems.System> Systems { get; }
    DbSet<User> Users { get; }
    DbSet<Module> Modules { get; }
    DbSet<Test> Tests { get; }
    DbSet<ModuleTest> ModuleTests { get; }
    DbSet<ConfigurationHome> ConfigurationHomes { get; }
    DbSet<ConfigurationSystem> ConfigurationSystems { get; }
    DbSet<ConfigurationModule> ConfigurationModules { get; }
    DbSet<ConfigurationInput> ConfigurationInput { get; }
    DbSet<HomeModule> HomeModules { get; }
    DbSet<HomeUser> HomeUsers { get; }
    DbSet<SystemModule> SystemModules { get; }
}