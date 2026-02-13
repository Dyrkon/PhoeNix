using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Configurations;

public class Configuration : AggregateRoot<ConfigurationId>
{
    private readonly List<Input> _inputs = new();
    private readonly List<ModuleValue> _modules = new();
    private readonly List<Systems.System> _systemsSpecifications = new();
    public string Title { get; private set; }
    public string Description { get; private set; }
    public IReadOnlyList<Input> Inputs => _inputs;
    public IReadOnlyList<ModuleValue> Modules => _modules;
    public IReadOnlyList<Systems.System> SystemSpecifications => _systemsSpecifications;

    private Configuration(ConfigurationId id) : base(id)
    {
    }

    public Result EditConfiguration(string? newTitle = null, string? newDescription = null)
    {
        if (newDescription is not null)
        {
            if (newDescription == string.Empty)
                return Result.Failure(new Error("", "Title can't be blank"));

            Description = newDescription;
        }

        if (newTitle is not null)
        {
            if (newTitle == string.Empty)
                return Result.Failure(new Error("", "Title can't be blank"));

            Title = newTitle;
        }

        return Result.Success();
    }


    public Result AddModule(ModuleTemplateId moduleTemplateId, bool enabled)
    {
        if (_modules.Any(h => h.ModuleTemplateId == moduleTemplateId))
            return Result.Failure(new Error("",
                $"This module ({moduleTemplateId}) is added already to this ({Title}) configuration"));

        return ModuleValue.Create(new ModuleValueId(Guid.NewGuid()), moduleTemplateId, enabled)
            .Tap(configurationModule => _modules.Add(configurationModule));
    }

    public Result ChangeModule(ModuleValueId moduleValueId, List<EntryValue> entries, string? content = null)
    {
        if (_modules.All(h => h.ModuleTemplateId != moduleValueId))
            return Result.Failure(
                new Error("", $"This module ({moduleValueId}) is not in this ({Title}) configuration"));

        return _modules.First(h => h.ModuleTemplateId == moduleValueId).ChangeValues(entries, content);
    }

    public Result RemoveModule(ModuleValueId moduleValueId)
    {
        var removedModules = _modules.RemoveAll(m => m.ModuleTemplateId == moduleValueId);
        if (removedModules == 0)
            return Result.Failure(new Error("",
                $"There is no module with id {moduleValueId} in this ({Title}) configuration"));

        return Result.Success();
    }

    public Result AddSystem(SystemId systemId, Architecture architecture, string name)
    {
        if (_systemsSpecifications.Any(h => h.Id == systemId))
            return Result.Failure(new Error("", $"This system ({systemId}) is added already"));

        return Systems.System.Create(new SystemId(systemId), architecture, name)
            .Tap(system => _systemsSpecifications.Add(system));
    }

    public Result ChangeSystemName(SystemId systemId, string newName)
    {
        if (_systemsSpecifications.All(h => h.Id != systemId))
            return Result.Failure(new Error("", $"There is no system with id {systemId}"));

        if (_systemsSpecifications.Any(h => h.Name == newName))
            return Result.Failure(new Error("",
                $"This system ({systemId}) with name {newName} is already in this ({Title}) configuration"));

        return _systemsSpecifications.First(s => s.Id == systemId).ChangeName(newName);
    }

    public Result AddSystemModule(SystemId systemId, ModuleTemplateId moduleTemplateId, bool enabled)
    {
        if (_systemsSpecifications.All(h => h.Id != systemId))
            return Result.Failure(new Error("", $"This system ({systemId}) is not in configuration {Title}"));

        var system = _systemsSpecifications.First(s => s.Id == systemId);
        return system.AddModule(moduleTemplateId, [system.Architecture], enabled);
    }

    public Result ChangeSystemModule(ModuleValueId moduleValueId, SystemId systemId, List<EntryValue> entries,
        string? content = null)
    {
        var system = _systemsSpecifications.FirstOrDefault(s => s.Id == systemId);
        if (system == null)
            return Result.Failure(new Error("", $"This system ({systemId}) is not in this ({Title}) configuration"));
        var module = _modules.FirstOrDefault(m => m.Id == moduleValueId);
        if (module == null)
            return Result.Failure(new Error("",
                $"This module ({moduleValueId}) is not in system ({systemId}) in configuration ({Title}) configuration"));

        return module.ChangeValues(entries, content);
    }

    public Result RemoveSystemModule(SystemId systemId, ModuleValueId moduleValueId)
    {
        if (_systemsSpecifications.All(h => h.Id != systemId))
            Result.Failure(new Error("", $"This system ({systemId}) is not in configuration {Title}"));

        return _systemsSpecifications.First(s => s.Id == systemId).RemoveModule(moduleValueId);
    }

    public Result RemoveSystem(SystemId systemId)
    {
        var removedSystems = _systemsSpecifications.RemoveAll(s => s.Id == systemId);
        if (removedSystems == 0)
            return Result.Failure(new Error("",
                $"There is no system with id {systemId} in this ({Title}) configuration"));

        return Result.Success();
    }

    public Result<Input> AddInput(string source, string name)
    {
        if (_inputs.Any(i => i.Name == name))
            return Result.Failure<Input>(new Error("",
                $"This input ({name}) is added to this ({Title}) configuration already"));

        return Input.Create(new InputId(Guid.NewGuid()), Id, source, name).Tap(i => _inputs.Add(i));
    }

    public Result AddInputFollow(InputId inputId, string followName, string followValue)
    {
        var input = _inputs.FirstOrDefault(i => i.Id == inputId);
        return input is null
            ? Result.Failure(new Error("ConfigurationInputCannotFindInput", $"Cannot find input {inputId.Value}"))
            : input.AddFollow(followName, followValue);
    }

    public Result RemoveInputFollow(Guid followId)
    {
        var input = _inputs.FirstOrDefault(i => i.Followers.Any(f => f.Id == followId));
        return input is null
            ? Result.Failure(
                new Error("ConfigurationInputCannotFindInput", $"Cannot find input with follow {followId}"))
            : input.RemoveFollow(followId);
    }

    public Result RemoveInput(InputId inputId)
    {
        var removeHomes = _inputs.RemoveAll(i => i.Id == inputId);
        if (removeHomes == 0)
            return Result.Failure(new Error("",
                $"There is no input with id {inputId} in this ({Title}) configuration"));

        return Result.Success();
    }

    public Result<List<Architecture>> SupportedSystemArchitectures()
    {
        if (_systemsSpecifications.Count == 0) return new List<Architecture>();
        List<Architecture> supportedArchitectures = new();
        if (_systemsSpecifications.All(s => s.Architecture == Architecture.X86Linux))
            supportedArchitectures.Add(Architecture.X86Linux);
        if (_systemsSpecifications.All(s => s.Architecture == Architecture.Aarch64Linux))
            supportedArchitectures.Add(Architecture.Aarch64Linux);
        if (_systemsSpecifications.All(s => s.Architecture == Architecture.X86Darwin))
            supportedArchitectures.Add(Architecture.X86Darwin);
        if (_systemsSpecifications.All(s => s.Architecture == Architecture.Aarch64Darwin))
            supportedArchitectures.Add(Architecture.Aarch64Darwin);

        return supportedArchitectures;
    }

    public static Result<Configuration> Create(ConfigurationId id, string title, string description)
    {
        return new Configuration(id) { Title = title, Description = description };
    }

    public Result<ConfigurationBuildResult> Build(List<ModuleTemplate> moduleTemplates)
    {
        var inputs = Inputs.Select(i => i.Build());
        if (inputs.Any(i => i.IsFailure))
            return Result.Failure<ConfigurationBuildResult>(new Error("",
                $"Failed to build input in configuration {Title}"));
        var modules = Modules.Select(m => m.Build(moduleTemplates.First(mt => mt.Id == m.ModuleTemplateId)));
        if (modules.Any(i => i.IsFailure))
            return Result.Failure<ConfigurationBuildResult>(new Error("",
                $"Failed to build module in configuration {Title}"));
        var systems = SystemSpecifications.Select(s => s.Build(moduleTemplates));
        if (systems.Any(i => i.IsFailure))
            return Result.Failure<ConfigurationBuildResult>(new Error("",
                $"Failed to build system in configuration {Title}"));
        var supportedArchitectures = SupportedSystemArchitectures();
        if (supportedArchitectures.IsFailure || supportedArchitectures.Value.Count == 0)
            return Result.Failure<ConfigurationBuildResult>(new Error("",
                $"Failed to get supported architectures for configuration {Title}"));

        var systemsPlaceholder = Guid.NewGuid().ToString();
        var sharedModulesPlaceholder = Guid.NewGuid().ToString();
        var checksPlaceholder = Guid.NewGuid().ToString();
        var inputsValues = inputs.Aggregate("", (current, result) => current + $"{result.Value.Input}\n");

        var content =
            $"{{ description = \"{Description}\"; " +
            $"inputs = {{ flake-utils.url = \"github:numtide/flake-utils\"; disko.url = \"github:nix-community/disko/latest\"; disko.inputs.nixpkgs.follows = \"nixpkgs\"; {inputsValues} }};\n " +
            $"outputs = {{self, nixpkgs, flake-utils, ...}} @ inputs: " +
            $"let\n systems = [{supportedArchitectures.Value.Aggregate("", (s, architecture) => $"\"{s + architecture.ToArchitectureString()}\" ")}];" +
            $"sharedModules = [ {sharedModulesPlaceholder} ];\n" +
            $"lib = nixpkgs.lib;" +
            $"in\n" +
            $"flake-utils.lib.eachSystem systems (system: let " +
            $"pkgs = nixpkgs.legacyPackages.${{system}}; \nin {{ \n" +
            $"formatter = pkgs.nixfmt;\n " +
            $"checks = {{ {checksPlaceholder} }};" +
            $"}}) // {{ nixosConfigurations = {{ {systemsPlaceholder} }}; }}; }}";

        return new ConfigurationBuildResult(Id, Title, content, sharedModulesPlaceholder,
            systemsPlaceholder, checksPlaceholder, supportedArchitectures.Value,
            modules.Select(m => m.Value),
            systems.Select(s => s.Value));
    }
}