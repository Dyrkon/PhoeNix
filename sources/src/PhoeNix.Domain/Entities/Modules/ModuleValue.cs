using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public sealed class ModuleValue : Entity<ModuleValueId>
{
    private readonly List<EntryValue> _editableValues = [];

    private ModuleValue(ModuleValueId id) : base(id)
    {
    }

    public ModuleTemplateId ModuleTemplateId { get; private set; } = default!;

    public bool Enabled { get; private set; }

    public IReadOnlyList<EntryValue> EditableValues => _editableValues;

    public Result Enable()
    {
        if (Enabled)
            return Result.Success();

        Enabled = true;
        return Result.Success();
    }

    public Result Disable()
    {
        if (!Enabled)
            return Result.Success();

        Enabled = false;
        return Result.Success();
    }

    public Result SetEnabled(bool enabled)
    {
        return enabled ? Enable() : Disable();
    }

    public Result ReplaceEntries(IReadOnlyCollection<EntryValue> entries)
    {
        if (entries.GroupBy(x => x.Name, StringComparer.Ordinal).Any(g => g.Count() > 1))
            return Result.Failure(new Error("Modules.DuplicateEntryName",
                "Entry names must be unique within a module value."));

        if (entries.GroupBy(x => x.Placeholder, StringComparer.Ordinal).Any(g => g.Count() > 1))
            return Result.Failure(new Error("Modules.DuplicatePlaceholder",
                "Entry placeholders must be unique within a module value."));

        _editableValues.Clear();
        _editableValues.AddRange(entries);

        return Result.Success();
    }

    public static Result<ModuleValue> Create(
        ModuleValueId moduleValueId,
        ModuleTemplateId moduleTemplateId,
        bool enabled = true)
    {
        return new ModuleValue(moduleValueId)
        {
            ModuleTemplateId = moduleTemplateId,
            Enabled = enabled
        };
    }
}