using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public sealed class SingleChoiceValue : EntryValue
{
    private readonly List<string> _options = [];

    private SingleChoiceValue()
    {
    }

    private SingleChoiceValue(
        EntryValueId id,
        string name,
        string placeholder,
        IEnumerable<string> options,
        string selectedValue)
        : base(id, name, placeholder)
    {
        _options.AddRange(options);
        Value = selectedValue;
    }

    public override EntryValueKind Kind => EntryValueKind.SingleChoice;

    public IReadOnlyList<string> Options => _options;

    public Result SetValue(string selectedValue)
    {
        if (!_options.Contains(selectedValue))
            return Result.Failure(new Error("Modules.SingleChoiceInvalid",
                $"Value '{selectedValue}' is not one of the allowed options."));

        Value = selectedValue;
        return Result.Success();
    }

    public static Result<SingleChoiceValue> Create(
        EntryValueId id,
        string name,
        string placeholder,
        IReadOnlyCollection<string> options,
        string selectedValue)
    {
        var normalizedOptions = options
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (normalizedOptions.Count == 0)
            return Result.Failure<SingleChoiceValue>(new Error("Modules.SingleChoiceNoOptions",
                "Single-choice entry must have at least one option."));

        if (!normalizedOptions.Contains(selectedValue))
            return Result.Failure<SingleChoiceValue>(new Error("Modules.SingleChoiceInvalid",
                $"Value '{selectedValue}' is not one of the allowed options."));

        return new SingleChoiceValue(id, name, placeholder, normalizedOptions, selectedValue);
    }
}