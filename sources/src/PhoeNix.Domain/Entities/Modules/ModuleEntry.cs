using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

// The editable values have guid placeholders in the content to be user editable 
public class ModuleEntry : Entity<ModuleEntryId>
{
    private readonly List<IEntryValue> _editableValues = new();

    private ModuleEntry(ModuleEntryId id) : base(id)
    {
    }

    public IReadOnlyList<IEntryValue> Editablevalues => _editableValues;

    public string Content { get; private set; }

    public Result EditContent(string content, List<IEntryValue> entries)
    {
        foreach (var entryValue in entries.Where(entryValue => !content.Contains(entryValue.Placeholder.ToString())))
            return Result.Failure(new Error("", $"Placeholder for value {entryValue.Name} is not present"));

        Content = content;
        _editableValues.Clear();
        _editableValues.AddRange(entries);

        return Result.Success();
    }

    public static Result<ModuleEntry> Create(ModuleEntryId id)
    {
        return new ModuleEntry(id) { Content = string.Empty };
    }
}