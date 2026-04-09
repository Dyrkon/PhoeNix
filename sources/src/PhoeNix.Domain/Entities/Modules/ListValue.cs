using System.Text.Json;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public sealed class ListValue : EntryValue
{
    private ListValue()
    {
    }

    private ListValue(EntryValueId id, string name, string placeholder, IReadOnlyList<string> items)
        : base(id, name, placeholder)
    {
        Value = JsonSerializer.Serialize(items);
    }

    public override EntryValueKind Kind => EntryValueKind.List;

    public IReadOnlyList<string> GetItems() =>
        string.IsNullOrEmpty(Value) ? [] : JsonSerializer.Deserialize<List<string>>(Value) ?? [];

    public override string GetNixExpression()
    {
        var items = GetItems();
        if (items.Count == 0) return "[ ]";
        var escaped = items.Select(i => $"\"{i.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"");
        return $"[ {string.Join(" ", escaped)} ]";
    }

    public static Result<ListValue> Create(
        EntryValueId id, string name, string placeholder, IReadOnlyList<string> items)
    {
        return new ListValue(id, name, placeholder, items);
    }
}
