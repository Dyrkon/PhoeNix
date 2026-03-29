using System.Globalization;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public sealed class DecimalRangeValue : EntryValue
{
    private DecimalRangeValue()
    {
    }

    private DecimalRangeValue(
        EntryValueId id,
        string name,
        string placeholder,
        decimal min,
        decimal max,
        decimal upperValue)
        : base(id, name, placeholder)
    {
        Min = min;
        Max = max;
        UpperValue = upperValue;
        Value = upperValue.ToString(CultureInfo.InvariantCulture);
    }

    public override EntryValueKind Kind => EntryValueKind.DecimalRange;

    public decimal UpperValue { get; private set; }

    public decimal? LowerValue { get; private set; }

    public decimal Min { get; private set; }

    public decimal Max { get; private set; }

    public Result SetValue(decimal upperValue, decimal? lowerValue = null)
    {
        if (Max < Min)
            return Result.Failure(new Error("Modules.DecimalRangeInvalid",
                $"Max '{Max}' has to be greater than or equal to min '{Min}'."));

        if (upperValue < Min || upperValue > Max)
            return Result.Failure(new Error("Modules.DecimalRangeOutOfBounds",
                $"Value '{upperValue}' is outside of interval <{Min}, {Max}>."));

        if (lowerValue.HasValue)
        {
            if (lowerValue.Value < Min || lowerValue.Value > Max)
                return Result.Failure(new Error("Modules.DecimalRangeOutOfBounds",
                    $"Lower value '{lowerValue.Value}' is outside of interval <{Min}, {Max}>."));

            if (upperValue < lowerValue.Value)
                return Result.Failure(new Error("Modules.DecimalRangeInvalid",
                    $"Upper value '{upperValue}' can't be smaller than lower value '{lowerValue.Value}'."));
        }

        UpperValue = upperValue;
        LowerValue = lowerValue;
        Value = upperValue.ToString(CultureInfo.InvariantCulture);

        return Result.Success();
    }

    public static Result<DecimalRangeValue> Create(
        EntryValueId id,
        string name,
        string placeholder,
        decimal min,
        decimal max,
        decimal upperValue,
        decimal? lowerValue = null)
    {
        if (max < min)
            return Result.Failure<DecimalRangeValue>(new Error("Modules.DecimalRangeInvalid",
                $"Max '{max}' has to be greater than or equal to min '{min}'."));

        var value = new DecimalRangeValue(id, name, placeholder, min, max, upperValue);
        var setResult = value.SetValue(upperValue, lowerValue);

        return setResult.IsFailure
            ? Result.Failure<DecimalRangeValue>(setResult.Error)
            : value;
    }
}