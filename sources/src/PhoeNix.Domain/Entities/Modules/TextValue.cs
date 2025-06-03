using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public class TextValue : EntryValue
{
    private TextValue(EntryValueId id)
    {
        Id = id;
    }

    public Result SetValue(string value)
    {
        Value = value;
        return Result.Success();
    }

    public static Result<TextValue> Create(EntryValueId id, string initialValue, string name, string placeHolder)
    {
        return new TextValue(id) { Value = initialValue, Placeholder = placeHolder, Name = name };
    }

    public static Result<TextValue> Create(EntryValueId id, string name, string placeHolder)
    {
        return new TextValue(id) { Value = placeHolder, Placeholder = placeHolder, Name = name };
    }
}