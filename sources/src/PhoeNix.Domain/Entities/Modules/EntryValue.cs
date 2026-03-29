using System.Text.Json;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Entities.Modules;

public abstract class EntryValue
{
    protected EntryValue()
    {
    }

    protected EntryValue(EntryValueId id, string name, string placeholder)
    {
        Id = id;
        Name = name;
        Placeholder = placeholder;
    }

    public EntryValueId Id { get; private set; } = default!;

    public string Name { get; private set; } = string.Empty;

    public string Placeholder { get; private set; } = string.Empty;

    public ModuleValueId ModuleValueId { get; private set; } = default!;

    public abstract EntryValueKind Kind { get; }

    public string Value { get; set; } = string.Empty;
}

public sealed record EntryValueDefinition(
    ModuleTemplateId ModuleTemplateId,
    string Name,
    string Placeholder,
    UserInputType InputType,
    EntryBindingKind BindingKind,
    EntryValueKind ValueKind,
    string? DefaultValue = null,
    string? DefaultLowerValue = null,
    int? IntegerMin = null,
    int? IntegerMax = null,
    decimal? DecimalMin = null,
    decimal? DecimalMax = null,
    bool AllowLowerValue = false,
    string? OptionsJson = null,
    int? BindingIndex = null)
{
    public IReadOnlyList<string> GetOptions()
    {
        if (string.IsNullOrWhiteSpace(OptionsJson))
            return [];

        return JsonSerializer.Deserialize<List<string>>(OptionsJson) ?? [];
    }
}