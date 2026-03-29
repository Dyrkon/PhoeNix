using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Configurations;

public sealed class Configuration : AggregateRoot<ConfigurationId>
{
    private readonly List<Input> _inputs = [];
    private readonly List<ModuleValue> _modules = [];
    private readonly List<Systems.System> _systemSpecifications = [];

    private Configuration(ConfigurationId id) : base(id)
    {
    }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public IReadOnlyList<Input> Inputs => _inputs;

    public IReadOnlyList<ModuleValue> Modules => _modules;

    public IReadOnlyList<Systems.System> SystemSpecifications => _systemSpecifications;

    public Result EditConfiguration(string? newTitle = null, string? newDescription = null)
    {
        if (newTitle is not null)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
                return Result.Failure(
                    new Error("Configurations.TitleEmpty", "Configuration title can't be blank."));

            Title = newTitle.Trim();
        }

        if (newDescription is not null)
        {
            if (string.IsNullOrWhiteSpace(newDescription))
                return Result.Failure(
                    new Error("Configurations.DescriptionEmpty", "Configuration description can't be blank."));

            Description = newDescription.Trim();
        }

        return Result.Success();
    }

    public Result<ModuleValue> AddModule(ModuleTemplateId moduleTemplateId, bool enabled)
    {
        if (_modules.Any(h => h.ModuleTemplateId == moduleTemplateId))
            return Result.Failure<ModuleValue>(
                new Error(
                    "Configurations.ModuleAlreadyAdded",
                    $"Module template '{moduleTemplateId.Value}' is already added to configuration '{Title}'."));

        return ModuleValue.Create(new ModuleValueId(Guid.NewGuid()), moduleTemplateId, enabled)
            .Tap(configurationModule => _modules.Add(configurationModule));
    }

    public Result<ModuleValue> UpdateModule(
        ModuleValueId moduleValueId,
        bool enabled,
        IReadOnlyCollection<EntryValue> entries)
    {
        var module = _modules.FirstOrDefault(m => m.Id == moduleValueId);

        if (module is null)
            return Result.Failure<ModuleValue>(
                new Error(
                    "Configurations.ModuleNotFound",
                    $"Module value '{moduleValueId.Value}' was not found in configuration '{Title}'."));

        return module.SetEnabled(enabled)
            .Tap(() => module.ReplaceEntries(entries))
            .Map(() => module);
    }

    public Result RemoveModule(ModuleValueId moduleValueId)
    {
        var removedModules = _modules.RemoveAll(m => m.Id == moduleValueId);

        if (removedModules == 0)
            return Result.Failure(
                new Error(
                    "Configurations.ModuleNotFound",
                    $"Module value '{moduleValueId.Value}' was not found in configuration '{Title}'."));

        return Result.Success();
    }

    public Result<Systems.System> AddSystem(SystemId systemId, Architecture architecture, string name)
    {
        if (_systemSpecifications.Any(h => h.Id == systemId))
            return Result.Failure<Systems.System>(
                new Error("Configurations.SystemAlreadyAdded", $"System '{systemId.Value}' is already added."));

        if (_systemSpecifications.Any(h => h.Name == name))
            return Result.Failure<Systems.System>(
                new Error(
                    "Configurations.SystemNameAlreadyUsed",
                    $"System with name '{name}' already exists in configuration '{Title}'."));

        return Systems.System.Create(systemId, Id, architecture, name)
            .Tap(system => _systemSpecifications.Add(system));
    }

    public Result<Systems.System> UpdateSystem(SystemId systemId, string newName)
    {
        var system = _systemSpecifications.FirstOrDefault(h => h.Id == systemId);

        if (system is null)
            return Result.Failure<Systems.System>(
                new Error(
                    "Configurations.SystemNotFound",
                    $"System '{systemId.Value}' was not found in configuration '{Title}'."));

        if (_systemSpecifications.Any(h => h.Id != systemId && h.Name == newName))
            return Result.Failure<Systems.System>(
                new Error(
                    "Configurations.SystemNameAlreadyUsed",
                    $"System with name '{newName}' already exists in configuration '{Title}'."));

        return system.ChangeName(newName)
            .Map(() => system);
    }

    public Result<ModuleValue> AddSystemModule(
        SystemId systemId,
        ModuleTemplateId moduleTemplateId,
        List<Architecture> supportedArchitectures,
        bool enabled)
    {
        var system = _systemSpecifications.FirstOrDefault(h => h.Id == systemId);

        if (system is null)
            return Result.Failure<ModuleValue>(
                new Error(
                    "Configurations.SystemNotFound",
                    $"System '{systemId.Value}' is not in configuration '{Title}'."));

        return system.AddModule(moduleTemplateId, supportedArchitectures, enabled);
    }

    public Result<ModuleValue> UpdateSystemModule(
        SystemId systemId,
        ModuleValueId moduleValueId,
        bool enabled,
        IReadOnlyCollection<EntryValue> entries)
    {
        var system = _systemSpecifications.FirstOrDefault(s => s.Id == systemId);

        if (system is null)
            return Result.Failure<ModuleValue>(
                new Error(
                    "Configurations.SystemNotFound",
                    $"System '{systemId.Value}' is not in configuration '{Title}'."));

        return system.UpdateModule(moduleValueId, enabled, entries);
    }

    public Result RemoveSystemModule(SystemId systemId, ModuleValueId moduleValueId)
    {
        var system = _systemSpecifications.FirstOrDefault(h => h.Id == systemId);

        if (system is null)
            return Result.Failure(
                new Error(
                    "Configurations.SystemNotFound",
                    $"System '{systemId.Value}' is not in configuration '{Title}'."));

        return system.RemoveModule(moduleValueId);
    }

    public Result RemoveSystem(SystemId systemId)
    {
        var removedSystems = _systemSpecifications.RemoveAll(s => s.Id == systemId);

        if (removedSystems == 0)
            return Result.Failure(
                new Error(
                    "Configurations.SystemNotFound",
                    $"System '{systemId.Value}' was not found in configuration '{Title}'."));

        return Result.Success();
    }

    public Result<Input> AddInput(string source, string name)
    {
        if (_inputs.Any(i => i.Name == name))
            return Result.Failure<Input>(
                new Error(
                    "Configurations.InputAlreadyAdded",
                    $"Input '{name}' is already added to configuration '{Title}'."));

        return Input.Create(new InputId(Guid.NewGuid()), Id, source, name)
            .Tap(i => _inputs.Add(i));
    }

    public Result<Input> UpdateInput(
        InputId inputId,
        string source,
        string name,
        IReadOnlyCollection<InputFollowDraft> follows)
    {
        var input = _inputs.FirstOrDefault(i => i.Id == inputId);

        if (input is null)
            return Result.Failure<Input>(
                new Error(
                    "Configurations.InputNotFound",
                    $"Input '{inputId.Value}' was not found in configuration '{Title}'."));

        if (_inputs.Any(i => i.Id != inputId && i.Name == name))
            return Result.Failure<Input>(
                new Error(
                    "Configurations.InputNameAlreadyUsed",
                    $"Input with name '{name}' already exists in configuration '{Title}'."));

        return input.ChangeSource(source)
            .Tap(() => input.ChangeName(name))
            .Tap(() => input.ReplaceFollows(follows))
            .Map(() => input);
    }

    public Result AddInputFollow(InputId inputId, string followName, string followValue)
    {
        var input = _inputs.FirstOrDefault(i => i.Id == inputId);

        return input is null
            ? Result.Failure(new Error("Configurations.InputNotFound", $"Cannot find input '{inputId.Value}'."))
            : input.AddFollow(followName, followValue);
    }

    public Result RemoveInputFollow(Guid followId)
    {
        var input = _inputs.FirstOrDefault(i => i.Followers.Any(f => f.Id == followId));

        return input is null
            ? Result.Failure(new Error("Configurations.InputFollowNotFound", $"Cannot find follow '{followId}'."))
            : input.RemoveFollow(followId);
    }

    public Result RemoveInput(InputId inputId)
    {
        var removedInputs = _inputs.RemoveAll(i => i.Id == inputId);

        if (removedInputs == 0)
            return Result.Failure(
                new Error(
                    "Configurations.InputNotFound",
                    $"Input '{inputId.Value}' was not found in configuration '{Title}'."));

        return Result.Success();
    }

    public IReadOnlyList<Architecture> SupportedSystemArchitectures()
    {
        return _systemSpecifications
            .Select(s => s.Architecture)
            .Distinct()
            .ToList();
    }

    public static Result<Configuration> Create(ConfigurationId id, string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Configuration>(
                new Error("Configurations.TitleEmpty", "Configuration title can't be blank."));

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<Configuration>(
                new Error("Configurations.DescriptionEmpty", "Configuration description can't be blank."));

        return new Configuration(id)
        {
            Title = title.Trim(),
            Description = description.Trim()
        };
    }
}