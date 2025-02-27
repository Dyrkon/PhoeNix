using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Systems;

public class System : AggregateRoot<SystemId>
{
    private readonly List<SystemModule> _modules = new();

    public Architecture Architecture { get; private set; }
    public IReadOnlyList<SystemModule> Modules => _modules;


    private System(SystemId id) : base(id)
    {
    }

    public Result AddModule(Module module)
    {
        if (_modules.Any(m => m.ModuleId == module.Id))
            return Result.Failure(new Error("", "This module has been added to this system already"));

        if (!module.SupportedArchitectures.Contains(Architecture))
            return Result.Failure(new Error("", $"This module doesn't support system architecture {Architecture}"));

        return SystemModule.Create(new SystemModuleId(Guid.NewGuid()), Id, module.Id)
            .Tap(m => _modules.Add(m));
    }

    public Result RemoveModule(ModuleId moduleId)
    {
        var removeHomes = _modules.RemoveAll(m => m.ModuleId == moduleId);
        if (removeHomes == 0)
            return Result.Failure(new Error("", $"There is no module with id {moduleId} in this system"));

        return Result.Success();
    }

    public static Result<System> Create(SystemId id, Architecture architecture)
    {
        return new System(id) { Architecture = architecture };
    }
}