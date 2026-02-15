using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public class ModuleValue : Entity<ModuleValueId>
{
    private readonly List<EntryValue> _editableValues = new();
    public ModuleTemplateId ModuleTemplateId { get; private set; }

    private ModuleValue(ModuleValueId id) : base(id)
    {
    }

    public bool Enabled { get; private set; }

    public IReadOnlyList<EntryValue> EditableValues => _editableValues;

    public Result Enable()
    {
        if (Enabled == true)
            return Result.Failure(new Error("", $"Module {Id.ToString()} is already enabled"));
        Enabled = true;
        return Result.Success();
    }

    public Result Disable()
    {
        if (Enabled == false)
            return Result.Failure(new Error("", $"Module {Id.ToString()} is already disabled"));
        Enabled = false;
        return Result.Success();
    }

    public Result ChangeValues(List<EntryValue> entries, string? content = null)
    {
        if (content != null)
        {
            foreach (var entryValue in entries.Where(entryValue => !content.Contains(entryValue.Name)))
                return Result.Failure(new Error("", $"Name for value {entryValue.Name} is not present"));
        }
        else
        {
            if (entries.Any(entry => _editableValues.First(i => i.Id == entry.Id).Name != entry.Name))
                return Result.Failure(new Error("", $"Changing module value names requires module template"));
        }

        _editableValues.Clear();
        _editableValues.AddRange(entries);

        return Result.Success();
    }

    public static Result<ModuleValue> Create(ModuleValueId moduleValueId, ModuleTemplateId moduleTemplateId,
        bool enabled = true)
    {
        return new ModuleValue(moduleValueId)
        {
            ModuleTemplateId = moduleTemplateId,
            Enabled = enabled
        };
    }
}