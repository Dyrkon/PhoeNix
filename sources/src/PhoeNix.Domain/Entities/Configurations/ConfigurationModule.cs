using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Configurations;

public class ConfigurationModule : Entity<ConfigurationModuleId>
{
    private readonly List<EntryValue> _editableValues = new();
    public ConfigurationId ConfigurationId { get; private set; }

    private ConfigurationModule(ConfigurationModuleId id) : base(id)
    {
    }

    public bool Enabled { get; private set; }

    public static Result<ConfigurationModule> Create()
    {
        return new ConfigurationModule(new ConfigurationModuleId(Guid.NewGuid()));
    }
}