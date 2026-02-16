using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Models.Configurations;

namespace PhoeNix.Application.Mappings;

public static class ConfigurationMappings
{
    public static ConfigurationResponse MapConfigurationToDto(Configuration flake)
    {
        return new ConfigurationResponse(
            flake.Id,
            flake.Title,
            flake.Description,
            flake.Inputs.Select(InputMappings.MapInputToDto).ToList(),
            flake.Modules.Select(ModuleMappings.MapModuleValueToDto).ToList(),
            flake.SystemSpecifications.Select(SystemMappings.MapSystemToListDto).ToList(),
            flake.SupportedSystemArchitectures().Value.ToList());
    }

    public static ConfigurationListResponse MapConfigurationToListDto(Configuration flake)
    {
        return new ConfigurationListResponse(flake.Id, flake.Title, flake.Description);
    }
}