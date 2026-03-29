using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Systems;

public sealed class System : Entity<SystemId>
{
    private readonly List<ModuleValue> _modules = [];

    private System(SystemId id) : base(id)
    {
    }

    public ConfigurationId ConfigurationId { get; private set; } = default!;

    public Architecture Architecture { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public IReadOnlyList<ModuleValue> Modules => _modules;

    public Result ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure(new Error("Systems.NameEmpty", "System name can't be empty."));

        Name = newName.Trim();
        return Result.Success();
    }

    public Result<ModuleValue> AddModule(
        ModuleTemplateId moduleTemplateId,
        List<Architecture> supportedArchitectures,
        bool enabled = true)
    {
        if (_modules.Any(m => m.ModuleTemplateId == moduleTemplateId))
            return Result.Failure<ModuleValue>(
                new Error("Systems.ModuleAlreadyAdded", "This module has already been added to this system."));

        if (!supportedArchitectures.Contains(Architecture))
            return Result.Failure<ModuleValue>(
                new Error(
                    "Systems.ModuleArchitectureMismatch",
                    $"This module doesn't support system architecture '{Architecture}'."));

        return ModuleValue.Create(new ModuleValueId(Guid.NewGuid()), moduleTemplateId, enabled)
            .Tap(m => _modules.Add(m));
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
                    "Systems.ModuleNotFound",
                    $"There is no module with id '{moduleValueId.Value}' in this system."));

        return module.SetEnabled(enabled)
            .Tap(() => module.ReplaceEntries(entries))
            .Map(() => module);
    }

    public Result RemoveModule(ModuleValueId moduleValueId)
    {
        var removedModules = _modules.RemoveAll(m => m.Id == moduleValueId);

        return removedModules == 0
            ? Result.Failure(
                new Error("Systems.ModuleNotFound",
                    $"There is no module with id '{moduleValueId.Value}' in this system."))
            : Result.Success();
    }

    public static Result<System> Create(
        SystemId id,
        ConfigurationId configurationId,
        Architecture architecture,
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<System>(new Error("Systems.NameEmpty", "System name can't be empty."));

        return new System(id)
        {
            ConfigurationId = configurationId,
            Architecture = architecture,
            Name = name.Trim()
        };
    }
}