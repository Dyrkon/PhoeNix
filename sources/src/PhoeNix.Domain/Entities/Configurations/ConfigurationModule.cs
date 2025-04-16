using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Configurations;

public class ConfigurationModule : Entity<ConfigurationModuleId>
{
    private ConfigurationModule(ConfigurationModuleId id) : base(id)
    {
    }

    public ConfigurationId ConfigurationId { get; private set; }

    public ModuleId ModuleId { get; private set; }

    public Configuration Configuration { get; private set; }

    public Module Module { get; private set; }

    public static Result<ConfigurationModule> Create(ConfigurationModuleId id, ConfigurationId configurationId,
        ModuleId moduleId)
    {
        return new ConfigurationModule(id)
        {
            ConfigurationId = configurationId,
            ModuleId = moduleId
        };
    }
}