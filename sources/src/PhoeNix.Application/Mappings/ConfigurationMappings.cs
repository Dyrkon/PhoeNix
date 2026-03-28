using PhoeNix.Application.Models.Configurations;
using PhoeNix.Domain.Entities.Configurations;

namespace PhoeNix.Application.Mappings;

public static class ConfigurationMappings
{
    public static ConfigurationResponse MapConfigurationToDto(Configuration configuration)
    {
        return new ConfigurationResponse(
            configuration.Id.Value,
            configuration.Title,
            configuration.Description,
            configuration.Inputs.Select(InputMappings.MapInputToDto).ToList(),
            configuration.Modules.Select(ModuleMappings.MapModuleValueToDto).ToList(),
            configuration.SystemSpecifications.Select(SystemMappings.MapSystemToListDto).ToList(),
            configuration.SupportedSystemArchitectures().ToList());
    }

    public static ConfigurationListResponse MapConfigurationToListDto(Configuration configuration)
    {
        return new ConfigurationListResponse(
            configuration.Id.Value,
            configuration.Title,
            configuration.Description);
    }
}