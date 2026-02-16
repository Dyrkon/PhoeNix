using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Models.Systems;

namespace PhoeNix.Application.Mappings;

public static class SystemMappings
{
    public static SystemListResponse MapSystemToListDto(Domain.Entities.Systems.System system)
    {
        return new SystemListResponse(
            system.Id,
            system.Architecture,
            system.Name,
            system.Modules.Select(ModuleMappings.MapModuleValueToListDto).ToList());
    }
}