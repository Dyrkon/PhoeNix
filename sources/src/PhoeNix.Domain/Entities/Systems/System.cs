using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Systems;

public class System : Entity<SystemId>
{
    private readonly List<ModuleValue> _modules = new();
    public ConfigurationId ConfigurationId;

    public Architecture Architecture { get; private set; }

    public string Name { get; private set; }

    public IReadOnlyList<ModuleValue> Modules => _modules;

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

    public Result AddModule(ModuleTemplateId moduleTemplateId, List<Architecture> supportedArchitectures,
        bool enabled = true)
    {
        if (_modules.Any(m => m.ModuleTemplateId == moduleTemplateId))
            return Result.Failure(new Error("", "This module has been added to this system already"));

        if (!supportedArchitectures.Contains(Architecture))
            return Result.Failure(new Error("", $"This module doesn't support system architecture {Architecture}"));

        return ModuleValue.Create(new ModuleValueId(Guid.NewGuid()), moduleTemplateId, enabled)
            .Tap(m => _modules.Add(m));
    }

    public Result RemoveModule(ModuleValueId moduleValueId)
    {
        var removeHomes = _modules.RemoveAll(m => m.Id == moduleValueId);
        if (removeHomes == 0)
            return Result.Failure(new Error("", $"There is no module with id {moduleValueId} in this system"));

        return Result.Success();
    }

    public static Result<System> Create(SystemId id, Architecture architecture, string name)
    {
        return new System(id) { Architecture = architecture, Name = name };
    }

    public Result<SystemBuildResult> Build(List<ModuleTemplate> moduleTemplates)
    {
        var modules = Modules.Select(m => m.Build(moduleTemplates.First(i => i.Id == m.ModuleTemplateId)));
        if (modules.Any(m => m.IsFailure))
            return Result.Failure<SystemBuildResult>(new Error("", $"Failed to build module/s for system {Name}"));

        var moduleResults = modules.Select(m => m.Value);
        var modulesListPlaceholder = Guid.NewGuid().ToString();
        // TODO Can't use lib.nixosSystem for darwin
        var systemContent =
            $"{{ inputs, lib, sharedModules }}:\ninputs.nixpkgs.lib.nixosSystem {{ specialArgs = {{ inherit inputs; }}; system = \"{Architecture.ToArchitectureString()}\"; modules = sharedModules ++ [ {modulesListPlaceholder} ]; }}";

        return new SystemBuildResult(Id, Name, Architecture, systemContent, moduleResults,
            modulesListPlaceholder);
    }
}