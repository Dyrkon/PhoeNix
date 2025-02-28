using PhoeNix.Domain.Entities.Flakes;
using PhoeNix.Domain.Models.Flakes;

namespace PhoeNix.Application.Mappings;

public static class FlakeMappings
{
    public static FlakeResponse MapFlakeToDto(Flake flake)
    {
        return new FlakeResponse(
            flake.Id,
            flake.Title,
            flake.Description,
            flake.Inputs.Select(InputMappings.MapInputToDto).ToList(),
            flake.Modules.Select(m => ModuleMappings.MapModuleToListDto(m.Module)).ToList(),
            flake.Systems.Select(s => SystemMappings.MapSystemToListDto(s.System)).ToList(),
            flake.Homes.Select(h => HomeMappings.MapHomeToListDto(h.Home)).ToList(),
            flake.SupportedSystemArchitectures().ToList()
        );
    }

    public static FlakeListResponse MapFlakeToListDto(Flake flake)
    {
        return new FlakeListResponse(flake.Id, flake.Title, flake.Description);
    }
}