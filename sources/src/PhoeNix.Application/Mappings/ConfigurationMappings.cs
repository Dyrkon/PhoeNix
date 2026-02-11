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
            flake.Inputs.Select(i => InputMappings.MapInputToDto(i.Input)).ToList(),
            flake.Modules.Select(m => ModuleMappings.MapModuleToListDto(m.Module)).ToList(),
            flake.Systems.Select(s => SystemMappings.MapSystemToListDto(s.System)).ToList(),
            flake.SupportedSystemArchitectures().Value.ToList());
    }

    public static ConfigurationListResponse MapConfigurationToListDto(Configuration flake)
    {
        return new ConfigurationListResponse(flake.Id, flake.Title, flake.Description);
    }
}