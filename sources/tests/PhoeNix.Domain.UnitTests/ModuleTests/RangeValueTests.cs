using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Domain.UnitTests.ModuleTests;

public class RangeValueTests
{
    private readonly string _placeholder = Guid.NewGuid().ToString();

    [Fact]
    public void IntegerRangeValue_Should_Create_With_Upper_And_Lower_Values()
    {
        var result = IntegerRangeValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "TempRange",
            _placeholder,
            min: 0,
            max: 100,
            upperValue: 80,
            lowerValue: 20);

        result.IsSuccess.Should().BeTrue();
        result.Value.UpperValue.Should().Be(80);
        result.Value.LowerValue.Should().Be(20);
        result.Value.Placeholder.Should().Be(_placeholder);
    }

    [Fact]
    public void IntegerRangeValue_Should_Fail_When_Max_Less_Than_Min()
    {
        var result = IntegerRangeValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "TempRange",
            _placeholder,
            min: 100,
            max: 0,
            upperValue: 50);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("greater than or equal to min");
    }

    [Fact]
    public void IntegerRangeValue_Should_Fail_When_Upper_Less_Than_Lower()
    {
        var result = IntegerRangeValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "TempRange",
            _placeholder,
            min: 0,
            max: 100,
            upperValue: 50,
            lowerValue: 60);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Upper value");
    }

    [Theory]
    [InlineData(150)]
    [InlineData(-10)]
    public void IntegerRangeValue_Should_Fail_SetValue_When_Outside_Bounds(int value)
    {
        var range = IntegerRangeValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "TempRange",
            _placeholder,
            min: 0,
            max: 100,
            upperValue: 50).Value;

        var result = range.SetValue(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("outside of interval");
    }

    [Fact]
    public void IntegerRangeValue_Should_SetValue_Successfully()
    {
        var range = IntegerRangeValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "TempRange",
            _placeholder,
            min: 0,
            max: 100,
            upperValue: 50).Value;

        var result = range.SetValue(60);

        result.IsSuccess.Should().BeTrue();
        range.UpperValue.Should().Be(60);
    }

    [Fact]
    public void IntegerRangeValue_Should_Fail_SetValue_When_Lower_Outside_Bounds()
    {
        var range = IntegerRangeValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "TempRange",
            _placeholder,
            min: 0,
            max: 100,
            upperValue: 50).Value;

        var result = range.SetValue(80, lowerValue: -5);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("outside of interval");
    }
}
