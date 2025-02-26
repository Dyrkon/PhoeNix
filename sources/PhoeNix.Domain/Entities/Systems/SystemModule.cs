using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Systems;

public class SystemModule : Entity<SystemModuleId>
{
    private SystemModule(SystemModuleId id) : base(id)
    {
    }

    public SystemId SystemId { get; private set; }

    public ModuleId ModuleId { get; private set; }

    public System System { get; private set; }

    public Module Module { get; private set; }

    public static Result<SystemModule> Create(SystemModuleId id, SystemId systemId, ModuleId moduleId)
    {
        return new SystemModule(id)
        {
            SystemId = systemId,
            ModuleId = moduleId
        };
    }
}