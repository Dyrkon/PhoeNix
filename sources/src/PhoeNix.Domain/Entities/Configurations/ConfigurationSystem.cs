using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Configurations;

public class ConfigurationSystem : Entity<ConfigurationSystemId>
{
    private ConfigurationSystem(ConfigurationSystemId id) : base(id)
    {
    }

    public ConfigurationId ConfigurationId { get; private set; }

    public SystemId SystemId { get; private set; }

    public Configuration Configuration { get; private set; }

    public Systems.System System { get; private set; }

    public static Result<ConfigurationSystem> Create(ConfigurationSystemId id, ConfigurationId configurationId,
        SystemId systemId)
    {
        return new ConfigurationSystem(id)
        {
            ConfigurationId = configurationId,
            SystemId = systemId
        };
    }

    internal void SetSystem(Systems.System system)
    {
        System = system;
    }
}