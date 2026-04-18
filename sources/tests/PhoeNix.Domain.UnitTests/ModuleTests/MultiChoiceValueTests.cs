using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Domain.UnitTests.ModuleTests;

public class SingleChoiceValueTests
{
    private readonly string _placeholder = Guid.NewGuid().ToString();

    [Fact]
    public void SingleChoiceValue_Should_Create_With_Valid_Default()
    {
        var result = SingleChoiceValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "Env",
            _placeholder,
            new[] { "dev", "prod" },
            "dev");

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("dev");
        result.Value.Placeholder.Should().Be(_placeholder);
        result.Value.Options.Should().BeEquivalentTo(new[] { "dev", "prod" });
    }

    [Fact]
    public void SingleChoiceValue_Should_Fail_When_Default_Not_In_Options()
    {
        var result = SingleChoiceValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "Mode",
            _placeholder,
            new[] { "a", "b" },
            "c");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("is not one of the allowed options");
    }

    [Fact]
    public void SingleChoiceValue_Should_Fail_When_No_Options()
    {
        var result = SingleChoiceValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "Mode",
            _placeholder,
            Array.Empty<string>(),
            "a");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("at least one option");
    }

    [Fact]
    public void SingleChoiceValue_Should_Set_Valid_Value()
    {
        var value = SingleChoiceValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "Theme",
            _placeholder,
            new[] { "dark", "light" },
            "light").Value;

        var result = value.SetValue("dark");

        result.IsSuccess.Should().BeTrue();
        value.Value.Should().Be("dark");
    }

    [Fact]
    public void SingleChoiceValue_Should_Fail_Set_Invalid_Value()
    {
        var value = SingleChoiceValue.Create(
            new EntryValueId(Guid.NewGuid()),
            "Switch",
            _placeholder,
            new[] { "on", "off" },
            "off").Value;

        var result = value.SetValue("maybe");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("is not one of the allowed options");
    }
}
