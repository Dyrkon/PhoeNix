using System.Data.SqlTypes;
using System.Numerics;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public class RangeValue<T> : IEntryValue where T : INumber<T>
{
    public EntryValueId Id { get; init; }
    public string Name { get; init; }
    public Guid Placeholder { get; init; }
    public string Value => UpperValue.ToString();

    public T UpperValue { get; private set; }

    public T? LowerValue { get; private set; }

    public T Max { get; private set; }

    public T Min { get; private set; }

    private RangeValue(EntryValueId id, T max, T min, T upperValue, Guid placeholder)
    {
        Id = id;
        UpperValue = upperValue;
        Max = max;
        Min = min;
        Placeholder = placeholder;
    }

    public Result SetValue(T upperValue, T lowerValue)
    {
        if (upperValue > Max || upperValue < Min)
            return Result.Failure(new Error("", $"Value {upperValue} is outside of <{Min},{Max}> interval"));

        if (lowerValue < Min || lowerValue > Max)
            return Result.Failure(new Error("", $"Value {lowerValue} is outside of <{Min},{Max}> interval"));

        if (upperValue < lowerValue)
            return Result.Failure(new Error("",
                $"Upper value {upperValue} can't be smaller than lower value {lowerValue}"));

        UpperValue = upperValue;
        LowerValue = lowerValue;
        return Result.Success();
    }

    public Result SetValue(T value)
    {
        if (value > Max || value < Min)
            return Result.Failure(new Error("", $"Value {value} is outside of <{Min},{Max}> interval"));

        UpperValue = value;

        return Result.Success();
    }

    public static Result<RangeValue<T>> Create(EntryValueId id, string name, Guid placeHolder, T max, T min,
        T upperValue, T lowerValue)
    {
        if (max < min || min > max)
            return Result.Failure<RangeValue<T>>(new Error("", $"Max {max} has to be larger than min {min}"));

        if (upperValue < lowerValue)
            return Result.Failure<RangeValue<T>>(new Error("",
                $"Upper value ({upperValue}) has to be bigger than lower value ({lowerValue})"));

        if (upperValue > max || upperValue < min || lowerValue > max || lowerValue < min)
            return Result.Failure<RangeValue<T>>(new Error("",
                $"Max {max} and min {min} have to belong in <{min},{max}> interval"));

        return new RangeValue<T>(id, max, min, upperValue, placeHolder) { LowerValue = lowerValue, Name = name };
    }

    public static Result<RangeValue<T>> Create(EntryValueId id, string name, Guid placeHolder, T max, T min, T value)
    {
        if (max < min || min > max)
            return Result.Failure<RangeValue<T>>(new Error("", $"Max {max} has to be larger than min {min}"));

        if (value > max || value < min)
            return Result.Failure<RangeValue<T>>(new Error("",
                $"Value {value} has to belong in <{min},{max}> interval"));

        return new RangeValue<T>(id, max, min, value, placeHolder) { Name = name };
    }
}