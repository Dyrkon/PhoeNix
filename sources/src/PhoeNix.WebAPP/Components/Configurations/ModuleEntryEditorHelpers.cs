using PhoeNix.Application.Models.Configurations;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Contracts;

namespace PhoeNix.WebAPP.Components.Configurations;

internal static class ModuleEntryEditorHelpers
{
    public static EntryEditModel BuildEntryModel(
        EntryValueDefinitionResponse def,
        ConfiguredModuleEntryResponse? current = null)
    {
        var model = new EntryEditModel
        {
            Name = def.Name,
            Placeholder = def.Placeholder,
            Kind = def.ValueKind,
            AllowLowerValue = def.AllowLowerValue
        };

        switch (def.ValueKind)
        {
            case EntryValueKind.Text:
                model.TextValue = current?.Value ?? def.DefaultValue ?? string.Empty;
                break;

            case EntryValueKind.IntegerRange:
                model.IntegerMin = def.IntegerMin ?? 0;
                model.IntegerMax = def.IntegerMax ?? 100;
                model.IntegerUpperValue = current?.IntegerUpperValue
                                          ?? TryParseInt(def.DefaultValue)
                                          ?? def.IntegerMin ?? 0;
                model.IntegerLowerValue = current?.IntegerLowerValue
                                          ?? TryParseInt(def.DefaultLowerValue);
                break;

            case EntryValueKind.DecimalRange:
                model.DecimalMin = def.DecimalMin ?? 0m;
                model.DecimalMax = def.DecimalMax ?? 100m;
                model.DecimalUpperValue = current?.DecimalUpperValue
                                          ?? TryParseDecimal(def.DefaultValue)
                                          ?? def.DecimalMin ?? 0m;
                model.DecimalLowerValue = current?.DecimalLowerValue
                                          ?? TryParseDecimal(def.DefaultLowerValue);
                break;

            case EntryValueKind.SingleChoice:
                model.Options = def.Options?.ToList() ?? [];
                model.SelectedValue = current?.Value
                                      ?? def.DefaultValue
                                      ?? model.Options.FirstOrDefault()
                                      ?? string.Empty;
                break;

            case EntryValueKind.List:
                model.ListItems = current?.ListItems?.ToList() ?? [];
                break;
        }

        return model;
    }

    public static ModuleEntryValueUpsertModel ToUpsertModel(EntryEditModel entry)
    {
        return entry.Kind switch
        {
            EntryValueKind.Text => new ModuleEntryValueUpsertModel(
                entry.Name, entry.Placeholder, entry.Kind,
                entry.TextValue, null, null, null, null, null),

            EntryValueKind.IntegerRange => new ModuleEntryValueUpsertModel(
                entry.Name, entry.Placeholder, entry.Kind,
                null, entry.IntegerUpperValue, entry.IntegerLowerValue, null, null, null),

            EntryValueKind.DecimalRange => new ModuleEntryValueUpsertModel(
                entry.Name, entry.Placeholder, entry.Kind,
                null, null, null, entry.DecimalUpperValue, entry.DecimalLowerValue, null),

            EntryValueKind.SingleChoice => new ModuleEntryValueUpsertModel(
                entry.Name, entry.Placeholder, entry.Kind,
                null, null, null, null, null, entry.SelectedValue),

            EntryValueKind.List => new ModuleEntryValueUpsertModel(
                entry.Name, entry.Placeholder, entry.Kind,
                null, null, null, null, null, null,
                entry.ListItems),

            _ => throw new InvalidOperationException($"Unsupported entry kind: {entry.Kind}")
        };
    }

    private static int? TryParseInt(string? value)
    {
        return int.TryParse(value, out var parsed) ? parsed : null;
    }

    private static decimal? TryParseDecimal(string? value)
    {
        return decimal.TryParse(value, out var parsed) ? parsed : null;
    }

    public sealed class EntryEditModel
    {
        public string Name { get; set; } = string.Empty;
        public string Placeholder { get; set; } = string.Empty;
        public EntryValueKind Kind { get; set; }

        public string TextValue { get; set; } = string.Empty;

        public int IntegerUpperValue { get; set; }
        public int? IntegerLowerValue { get; set; }
        public int IntegerMin { get; set; }
        public int IntegerMax { get; set; }
        public bool AllowLowerValue { get; set; }

        public decimal DecimalUpperValue { get; set; }
        public decimal? DecimalLowerValue { get; set; }
        public decimal DecimalMin { get; set; }
        public decimal DecimalMax { get; set; }

        public string SelectedValue { get; set; } = string.Empty;
        public List<string> Options { get; set; } = [];

        public List<string> ListItems { get; set; } = [];
    }
}