using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Configurations;

public class ConfigurationHome : Entity<ConfigurationHomeId>
{
    private ConfigurationHome(ConfigurationHomeId id) : base(id)
    {
    }

    public ConfigurationId ConfigurationId { get; private set; }

    public HomeId HomeId { get; private set; }

    public Configuration Configuration { get; private set; }

    public Home Home { get; private set; }

    public static Result<ConfigurationHome> Create(ConfigurationHomeId id, ConfigurationId configurationId,
        HomeId configurationHomeId)
    {
        return new ConfigurationHome(id)
        {
            ConfigurationId = configurationId,
            HomeId = configurationHomeId
        };
    }

    internal void SetHome(Home home)
    {
        Home = home;
    }
}