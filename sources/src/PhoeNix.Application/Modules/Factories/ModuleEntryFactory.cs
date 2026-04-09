using System.Globalization;
using System.Text.Json;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Modules.Factories;

internal static class ModuleEntryFactory
{
    public static Result<IReadOnlyCollection<EntryValue>> CreateDefaultEntries(
        ModuleValue moduleValue,
        IReadOnlyCollection<EntryValueDefinition> definitions)
    {
        var entries = new List<EntryValue>();

        foreach (var definition in definitions)
        {
            var result = CreateEntry(
                definition,
                null,
                FindExistingEntryId(moduleValue, definition.Name) ?? new EntryValueId(Guid.NewGuid()));

            if (result.IsFailure)
                return Result.Failure<IReadOnlyCollection<EntryValue>>(result.Error);

            entries.Add(result.Value);
        }

        return entries;
    }

    public static Result<IReadOnlyCollection<EntryValue>> CreateEntries(
        ModuleValue moduleValue,
        IReadOnlyCollection<EntryValueDefinition> definitions,
        IReadOnlyCollection<ModuleEntryValueUpsertModel> requestEntries)
    {
        if (requestEntries.GroupBy(x => x.Name, StringComparer.Ordinal).Any(g => g.Count() > 1))
            return Result.Failure<IReadOnlyCollection<EntryValue>>(new Error("Modules.DuplicateEntryName",
                "Entry names must be unique within the request."));

        if (definitions.Count != requestEntries.Count)
            return Result.Failure<IReadOnlyCollection<EntryValue>>(new Error("Modules.EntryCountMismatch",
                "Request entry count does not match template definition count."));

        var entries = new List<EntryValue>();

        foreach (var definition in definitions)
        {
            var requestEntry = requestEntries.FirstOrDefault(x => x.Name == definition.Name);

            if (requestEntry is null)
                return Result.Failure<IReadOnlyCollection<EntryValue>>(
                    new Error("Modules.EntryMissing", $"Entry '{definition.Name}' is missing from the request."));

            if (requestEntry.Placeholder != definition.Placeholder)
                return Result.Failure<IReadOnlyCollection<EntryValue>>(
                    new Error("Modules.PlaceholderMismatch",
                        $"Entry '{definition.Name}' does not match template placeholder."));

            if (requestEntry.Kind != definition.ValueKind)
                return Result.Failure<IReadOnlyCollection<EntryValue>>(
                    new Error("Modules.EntryKindMismatch",
                        $"Entry '{definition.Name}' does not match template kind '{definition.ValueKind}'."));

            var result = CreateEntry(
                definition,
                requestEntry,
                FindExistingEntryId(moduleValue, definition.Name) ?? new EntryValueId(Guid.NewGuid()));

            if (result.IsFailure)
                return Result.Failure<IReadOnlyCollection<EntryValue>>(result.Error);

            entries.Add(result.Value);
        }

        return entries;
    }

    private static EntryValueId? FindExistingEntryId(ModuleValue moduleValue, string entryName)
    {
        return moduleValue.EditableValues.FirstOrDefault(x => x.Name == entryName)?.Id;
    }

    private static Result<EntryValue> CreateEntry(
        EntryValueDefinition definition,
        ModuleEntryValueUpsertModel? requestEntry,
        EntryValueId entryValueId)
    {
        return definition.ValueKind switch
        {
            EntryValueKind.Text => CreateTextEntry(definition, requestEntry, entryValueId),
            EntryValueKind.IntegerRange => CreateIntegerRangeEntry(definition, requestEntry, entryValueId),
            EntryValueKind.DecimalRange => CreateDecimalRangeEntry(definition, requestEntry, entryValueId),
            EntryValueKind.SingleChoice => CreateSingleChoiceEntry(definition, requestEntry, entryValueId),
            EntryValueKind.List => CreateListEntry(definition, requestEntry, entryValueId),
            _ => Result.Failure<EntryValue>(new Error("Modules.UnsupportedEntryKind",
                $"Unsupported entry kind '{definition.ValueKind}'."))
        };
    }

    private static Result<EntryValue> CreateTextEntry(
        EntryValueDefinition definition,
        ModuleEntryValueUpsertModel? requestEntry,
        EntryValueId entryValueId)
    {
        var value = requestEntry?.TextValue ?? definition.DefaultValue ?? string.Empty;

        return TextValue.Create(
                entryValueId,
                value,
                definition.Name,
                definition.Placeholder)
            .Map<TextValue, EntryValue>(x => x);
    }

    private static Result<EntryValue> CreateIntegerRangeEntry(
        EntryValueDefinition definition,
        ModuleEntryValueUpsertModel? requestEntry,
        EntryValueId entryValueId)
    {
        if (definition.IntegerMin is null || definition.IntegerMax is null)
            return Result.Failure<EntryValue>(
                new Error("Modules.IntegerRangeDefinitionInvalid",
                    $"Entry '{definition.Name}' requires IntegerMin and IntegerMax."));

        var upperValue = requestEntry?.IntegerUpperValue
                         ?? ParseInt(definition.DefaultValue, definition.Name, "DefaultValue")
                         ?? definition.IntegerMin.Value;

        int? lowerValue = null;

        if (definition.AllowLowerValue)
            lowerValue = requestEntry?.IntegerLowerValue
                         ?? ParseInt(definition.DefaultLowerValue, definition.Name, "DefaultLowerValue");

        return IntegerRangeValue.Create(
                entryValueId,
                definition.Name,
                definition.Placeholder,
                definition.IntegerMin.Value,
                definition.IntegerMax.Value,
                upperValue,
                lowerValue)
            .Map<IntegerRangeValue, EntryValue>(x => x);
    }

    private static Result<EntryValue> CreateDecimalRangeEntry(
        EntryValueDefinition definition,
        ModuleEntryValueUpsertModel? requestEntry,
        EntryValueId entryValueId)
    {
        if (definition.DecimalMin is null || definition.DecimalMax is null)
            return Result.Failure<EntryValue>(
                new Error("Modules.DecimalRangeDefinitionInvalid",
                    $"Entry '{definition.Name}' requires DecimalMin and DecimalMax."));

        var upperValue = requestEntry?.DecimalUpperValue
                         ?? ParseDecimal(definition.DefaultValue, definition.Name, "DefaultValue")
                         ?? definition.DecimalMin.Value;

        decimal? lowerValue = null;

        if (definition.AllowLowerValue)
            lowerValue = requestEntry?.DecimalLowerValue
                         ?? ParseDecimal(definition.DefaultLowerValue, definition.Name, "DefaultLowerValue");

        return DecimalRangeValue.Create(
                entryValueId,
                definition.Name,
                definition.Placeholder,
                definition.DecimalMin.Value,
                definition.DecimalMax.Value,
                upperValue,
                lowerValue)
            .Map<DecimalRangeValue, EntryValue>(x => x);
    }

    private static Result<EntryValue> CreateSingleChoiceEntry(
        EntryValueDefinition definition,
        ModuleEntryValueUpsertModel? requestEntry,
        EntryValueId entryValueId)
    {
        var options = definition.GetOptions();

        if (options.Count == 0)
            return Result.Failure<EntryValue>(
                new Error("Modules.SingleChoiceDefinitionInvalid",
                    $"Entry '{definition.Name}' requires at least one option."));

        var selectedValue = requestEntry?.SelectedValue
                            ?? definition.DefaultValue
                            ?? options[0];

        return SingleChoiceValue.Create(
                entryValueId,
                definition.Name,
                definition.Placeholder,
                options,
                selectedValue)
            .Map<SingleChoiceValue, EntryValue>(x => x);
    }

    private static Result<EntryValue> CreateListEntry(
        EntryValueDefinition definition,
        ModuleEntryValueUpsertModel? requestEntry,
        EntryValueId entryValueId)
    {
        IReadOnlyList<string> items = requestEntry?.ListItems is { Count: > 0 } provided
            ? provided
            : (!string.IsNullOrEmpty(definition.DefaultValue)
                ? JsonSerializer.Deserialize<List<string>>(definition.DefaultValue) ?? []
                : []);

        return ListValue.Create(entryValueId, definition.Name, definition.Placeholder, items)
            .Map<ListValue, EntryValue>(x => x);
    }

    private static int? ParseInt(string? value, string entryName, string source)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            return parsed;

        throw new InvalidOperationException($"Entry '{entryName}' contains invalid integer in '{source}'.");
    }

    private static decimal? ParseDecimal(string? value, string entryName, string source)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            return parsed;

        throw new InvalidOperationException($"Entry '{entryName}' contains invalid decimal in '{source}'.");
    }
}