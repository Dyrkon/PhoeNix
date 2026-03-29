using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public sealed class TextValue : EntryValue
{
    private TextValue()
    {
    }

    private TextValue(EntryValueId id, string name, string placeholder, string value)
        : base(id, name, placeholder)
    {
        Value = value;
    }

    public override EntryValueKind Kind => EntryValueKind.Text;

    public Result SetValue(string value)
    {
        Value = value;
        return Result.Success();
    }

    public static Result<TextValue> Create(EntryValueId id, string value, string name, string placeholder)
    {
        return new TextValue(id, name, placeholder, value);
    }
}