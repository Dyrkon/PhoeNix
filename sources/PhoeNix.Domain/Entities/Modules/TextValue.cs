using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public class TextValue : IEntryValue
{
    private string _value = string.Empty;

    private TextValue(EntryValueId id)
    {
        Id = id;
    }

    public EntryValueId Id { get; init; }
    public string Name { get; init; }
    public Guid Placeholder { get; init; }
    public string Value => _value;

    public Result SetValue(string value)
    {
        _value = value;
        return Result.Success();
    }

    public static Result<TextValue> Create(EntryValueId id, string initialValue, string name, Guid placeHolder)
    {
        return new TextValue(id) { _value = initialValue, Placeholder = placeHolder, Name = name };
    }
}