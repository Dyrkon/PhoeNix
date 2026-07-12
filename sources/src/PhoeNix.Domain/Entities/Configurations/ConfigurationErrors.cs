using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Configurations;

public static class ConfigurationErrors
{
    public static Error NotFound()
    {
        return new Error("Configurations.NotFound", $"Configuration was not found.");
    }

    public static Error NotFound(ConfigurationId configurationId)
    {
        return new Error("Configurations.NotFound", $"Configuration '{configurationId}' was not found.");
    }

    public static Error TitleAlreadyExists(string title)
    {
        return new Error("Configurations.TitleAlreadyExists", $"A configuration with title '{title}' already exists.");
    }
}