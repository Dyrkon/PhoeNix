using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Domain.UnitTests.ModuleTests;

public class DecimalRangeValueTests
{
    private readonly string _placeholder = Guid.NewGuid().ToString();

    [Fact]
    public void DecimalRangeValue_Should_Create_With_Upper_And_Lower_Values()
    {
        var result = DecimalRangeValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "TempRange",
            _placeholder,
            min: 0m,
            max: 100m,
            upperValue: 75.5m,
            lowerValue: 25.5m);

        result.IsSuccess.Should().BeTrue();
        result.Value.UpperValue.Should().Be(75.5m);
        result.Value.LowerValue.Should().Be(25.5m);
        result.Value.Min.Should().Be(0m);
        result.Value.Max.Should().Be(100m);
        result.Value.Placeholder.Should().Be(_placeholder);
    }

    [Fact]
    public void DecimalRangeValue_Should_Create_Without_LowerValue()
    {
        var result = DecimalRangeValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "Range",
            _placeholder,
            min: 0m,
            max: 10m,
            upperValue: 5m);

        result.IsSuccess.Should().BeTrue();
        result.Value.LowerValue.Should().BeNull();
    }

    [Fact]
    public void DecimalRangeValue_Should_Fail_When_Max_Less_Than_Min()
    {
        var result = DecimalRangeValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "Range",
            _placeholder,
            min: 100m,
            max: 0m,
            upperValue: 50m);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("greater than or equal to min");
    }

    [Fact]
    public void DecimalRangeValue_Should_Fail_When_Upper_Less_Than_Lower()
    {
        var result = DecimalRangeValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "Range",
            _placeholder,
            min: 0m,
            max: 100m,
            upperValue: 30m,
            lowerValue: 50m);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Upper value");
    }

    [Theory]
    [InlineData(150.0)]
    [InlineData(-1.0)]
    public void DecimalRangeValue_Should_Fail_SetValue_When_Outside_Bounds(double rawValue)
    {
        var value = (decimal)rawValue;
        var range = DecimalRangeValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "Range",
            _placeholder,
            min: 0m,
            max: 100m,
            upperValue: 50m).Value;

        var result = range.SetValue(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("outside of interval");
    }

    [Fact]
    public void DecimalRangeValue_Should_SetValue_Successfully()
    {
        var range = DecimalRangeValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "Range",
            _placeholder,
            min: 0m,
            max: 100m,
            upperValue: 50m).Value;

        var result = range.SetValue(66.6m);

        result.IsSuccess.Should().BeTrue();
        range.UpperValue.Should().Be(66.6m);
    }

    [Fact]
    public void DecimalRangeValue_Should_Fail_SetValue_When_Lower_Outside_Bounds()
    {
        var range = DecimalRangeValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "Range",
            _placeholder,
            min: 0m,
            max: 100m,
            upperValue: 50m).Value;

        var result = range.SetValue(80m, lowerValue: -5m);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("outside of interval");
    }
}
