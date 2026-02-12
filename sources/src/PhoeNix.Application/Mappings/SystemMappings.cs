using PhoeNix.Domain.Models.Systems;

namespace PhoeNix.Application.Mappings;

public static class SystemMappings
{
    public static SystemListResponse MapSystemToListDto(Domain.Entities.Systems.System system)
    {
        return new SystemListResponse(system.Id, system.Name, system.Architecture);
    }

    public static SystemResponse MapSystemToDto(Domain.Entities.Systems.System system)
    {
        return new SystemResponse(system.Id, system.Name, system.Architecture,
            system.Modules.Select(m => ModuleMappings.MapModuleToListDto(m.ModuleTemplate)).ToList());
    }
}