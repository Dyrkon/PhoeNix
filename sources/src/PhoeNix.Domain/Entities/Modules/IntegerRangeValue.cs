using System.Globalization;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public sealed class IntegerRangeValue : EntryValue
{
    private IntegerRangeValue()
    {
    }

    private IntegerRangeValue(
        EntryValueId id,
        string name,
        string placeholder,
        int min,
        int max,
        int upperValue)
        : base(id, name, placeholder)
    {
        Min = min;
        Max = max;
        UpperValue = upperValue;
        Value = upperValue.ToString(CultureInfo.InvariantCulture);
    }

    public override EntryValueKind Kind => EntryValueKind.IntegerRange;

    public int UpperValue { get; private set; }

    public int? LowerValue { get; private set; }

    public int Min { get; private set; }

    public int Max { get; private set; }

    public Result SetValue(int upperValue, int? lowerValue = null)
    {
        if (Max < Min)
            return Result.Failure(new Error("Modules.IntegerRangeInvalid",
                $"Max '{Max}' has to be greater than or equal to min '{Min}'."));

        if (upperValue < Min || upperValue > Max)
            return Result.Failure(new Error("Modules.IntegerRangeOutOfBounds",
                $"Value '{upperValue}' is outside of interval <{Min}, {Max}>."));

        if (lowerValue.HasValue)
        {
            if (lowerValue.Value < Min || lowerValue.Value > Max)
                return Result.Failure(new Error("Modules.IntegerRangeOutOfBounds",
                    $"Lower value '{lowerValue.Value}' is outside of interval <{Min}, {Max}>."));

            if (upperValue < lowerValue.Value)
                return Result.Failure(new Error("Modules.IntegerRangeInvalid",
                    $"Upper value '{upperValue}' can't be smaller than lower value '{lowerValue.Value}'."));
        }

        UpperValue = upperValue;
        LowerValue = lowerValue;
        Value = upperValue.ToString(CultureInfo.InvariantCulture);

        return Result.Success();
    }

    public static Result<IntegerRangeValue> Create(
        EntryValueId id,
        string name,
        string placeholder,
        int min,
        int max,
        int upperValue,
        int? lowerValue = null)
    {
        if (max < min)
            return Result.Failure<IntegerRangeValue>(new Error("Modules.IntegerRangeInvalid",
                $"Max '{max}' has to be greater than or equal to min '{min}'."));

        var value = new IntegerRangeValue(id, name, placeholder, min, max, upperValue);
        var setResult = value.SetValue(upperValue, lowerValue);

        return setResult.IsFailure
            ? Result.Failure<IntegerRangeValue>(setResult.Error)
            : value;
    }
}