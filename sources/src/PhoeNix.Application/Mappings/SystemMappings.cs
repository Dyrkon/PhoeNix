using PhoeNix.Contracts.Systems;
using PhoeNix.Domain.Entities.Systems;

namespace PhoeNix.Application.Mappings;

public static class SystemMappings
{
    public static SystemListResponse MapSystemToListDto(Domain.Entities.Systems.System system)
    {
        return new SystemListResponse(
            system.Id.Value,
            system.Name,
            system.Architecture);
    }

    public static SystemResponse MapSystemToDto(Domain.Entities.Systems.System system)
    {
        return new SystemResponse(
            system.Id.Value,
            system.Name,
            system.Architecture,
            system.Modules
                .Select(ModuleMappings.MapModuleValueToDto)
                .ToList());
    }
}