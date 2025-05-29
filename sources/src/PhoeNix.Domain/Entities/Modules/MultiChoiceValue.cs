using System.Runtime.InteropServices;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public class MultiChoiceValue<T> : EntryValue
{
    private readonly List<T> _options = new();

    private MultiChoiceValue(EntryValueId id)
    {
        Id = id;
    }

    public IReadOnlyList<T> Options => _options;

    public Result SetValue(T value)
    {
        if (!_options.Contains(value)) return Result.Failure(new Error("", $"Value {value} is not one of the options"));

        Value = value?.ToString() ?? string.Empty;
        return Result.Success();
    }

    public Result AddOption(T option)
    {
        if (_options.Contains(option))
            return Result.Failure(new Error("", $"Option {option} is added already"));

        _options.Add(option);

        return Result.Success();
    }

    public Result RemoveOption(T option)
    {
        var removed = _options.RemoveAll(o => o.Equals(option));
        if (removed == 0)
            return Result.Failure(new Error("", $"Option {option} is not part of the options"));

        return Result.Success();
    }

    public Result AddOptions(List<T> options)
    {
        var addedAlready = _options.Intersect(options).ToList();
        if (addedAlready.Count != 0)
            return Result.Failure(new Error("", $"Options {addedAlready} have been added already"));

        _options.AddRange(options);
        return Result.Success();
    }

    public static Result<MultiChoiceValue<T>> Create(EntryValueId id, List<T> options, T defaultValue,
        string placeHolder,
        string name)
    {
        if (!options.Contains(defaultValue))
            return Result.Failure<MultiChoiceValue<T>>(new Error("",
                $"Default value {defaultValue} is not present in options"));

        return new Result<MultiChoiceValue<T>>(true, Error.None,
                new MultiChoiceValue<T>(id)
                    { Placeholder = placeHolder, Name = name, Value = defaultValue?.ToString() ?? string.Empty })
            .Tap(o =>
                o.AddOptions(options));
    }
}