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

    public string Name { get; private set; }

    public IReadOnlyList<SystemModule> Modules => _modules;

    private System(SystemId id) : base(id)
    {
    }

    public Result ChangeName(string newName)
    {
        if (newName == string.Empty)
            return Result.Failure(new Error("", $"System name can't be empty"));

        Name = newName;
        return Result.Success();
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

    public static Result<System> Create(SystemId id, Architecture architecture, string name)
    {
        return new System(id) { Architecture = architecture, Name = name };
    }

    public Result<SystemBuildResult> Build()
    {
        var modules = Modules.Select(m => m.Module.Build());
        if (modules.Any(m => m.IsFailure))
            return Result.Failure<SystemBuildResult>(new Error("", $"Failed to build module/s for system {Name}"));

        var moduleResults = modules.Select(m => m.Value);
        var modulesListPlaceholder = Guid.NewGuid().ToString();
        var systemContent =
            $"{{ inputs, sharedModules }}:\ninputs.nixpkgs.lib.nixosSystem {{ system = {Architecture.ToArchitectureString()}; modules = sharedModules ++ [ {modulesListPlaceholder} ]; }}";


        return new SystemBuildResult(Name, Architecture, systemContent, moduleResults, modulesListPlaceholder);
    }
}