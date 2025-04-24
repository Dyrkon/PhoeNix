using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Domain.UnitTests.ModuleTests;

public class RangeValueTests
{
    private readonly string placeholder = Guid.NewGuid().ToString();

    [Fact]
    public void RangeValue_Should_Store_Values_And_Respect_Range()
    {
        var result = RangeValue<int>.Create(new EntryValueId(Guid.NewGuid()), "TempRange", placeholder, 100, 0, 80, 20);

        result.IsSuccess.Should().BeTrue();
        result.Value.UpperValue.Should().Be(80);
        result.Value.LowerValue.Should().Be(20);
        result.Value.Placeholder.Should().Be(placeholder);
    }

    [Fact]
    public void RangeValue_Should_Fail_When_Upper_Is_Less_Than_Lower()
    {
        var range = RangeValue<int>.Create(new EntryValueId(Guid.NewGuid()), "TempRange", placeholder, 100, 0, 50, 60);

        range.IsFailure.Should().BeTrue();
        range.Error.Description.Should().Contain("Upper value");
    }

    [Theory]
    [InlineData(150)]
    [InlineData(-10)]
    public void RangeValue_Should_Fail_When_Value_Outside_Bounds(int value)
    {
        var range = RangeValue<int>.Create(new EntryValueId(Guid.NewGuid()), "TempRange", placeholder, 100, 0, 50)
            .Value;

        var result = range.SetValue(value);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("outside");
    }

    [Fact]
    public void RangeValue_Should_Set_Single_Value_Successfully()
    {
        var range = RangeValue<int>.Create(new EntryValueId(Guid.NewGuid()), "TempRange", placeholder, 100, 0, 50)
            .Value;

        var result = range.SetValue(60);

        result.IsSuccess.Should().BeTrue();
        range.Value.Should().Be("60");
    }
}