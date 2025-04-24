using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Domain.UnitTests.ModuleTests;

public class MultiChoiceValueTests
{
    private readonly string placeholder = Guid.NewGuid().ToString();

    [Fact]
    public void MultiChoiceValue_Should_Create_With_Default_Option()
    {
        var result = MultiChoiceValue<string>.Create(new EntryValueId(Guid.NewGuid()), ["dev", "prod"], "dev",
            placeholder, "Env");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("dev");
        result.Value.Placeholder.Should().Be(placeholder);
    }

    [Fact]
    public void MultiChoiceValue_Should_Fail_With_Invalid_Default()
    {
        var result =
            MultiChoiceValue<string>.Create(new EntryValueId(Guid.NewGuid()), ["a", "b"], "c", placeholder, "Mode");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Default value");
    }

    [Fact]
    public void MultiChoiceValue_Should_Set_Valid_Value()
    {
        var value = MultiChoiceValue<string>
            .Create(new EntryValueId(Guid.NewGuid()), ["dark", "light"], "light", placeholder, "Theme").Value;

        var result = value.SetValue("dark");

        result.IsSuccess.Should().BeTrue();
        value.Value.Should().Be("dark");
    }

    [Fact]
    public void MultiChoiceValue_Should_Fail_Set_Invalid_Value()
    {
        var value = MultiChoiceValue<string>
            .Create(new EntryValueId(Guid.NewGuid()), ["on", "off"], "off", placeholder, "Switch").Value;

        var result = value.SetValue("maybe");

        result.IsFailure.Should().BeTrue();
    }
}