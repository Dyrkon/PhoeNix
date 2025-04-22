using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Configurations;

public class ConfigurationInput : Entity<ConfigurationInputId>
{
    private ConfigurationInput(ConfigurationInputId id) : base(id)
    {
    }
    
    public ConfigurationId ConfigurationId { get; private set; }
    
    public InputId InputId  { get; private set; }
    
    public Configuration Configuration { get; private set; }
    
    public Input Input  { get; private set; }

    public static Result<ConfigurationInput> Create(ConfigurationInputId id, ConfigurationId configurationId, InputId inputId)
    {
        return new ConfigurationInput(id)
        {
            ConfigurationId = configurationId,
            InputId = inputId
        };
    }
    
    internal void SetInput(Input input)
    {
        Input = input;
    }
}