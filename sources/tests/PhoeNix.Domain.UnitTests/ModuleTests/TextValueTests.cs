using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Domain.UnitTests.ModuleTests;

public class TextValueTests
{
    private readonly string placeholder = Guid.NewGuid().ToString();

    [Fact]
    public void TextValue_Should_Store_Value_And_Placeholder()
    {
        var value = TextValue.Create(new EntryValueId(Guid.NewGuid()), "init", "Username", placeholder).Value;

        value.Value.Should().Be("init");
        value.Placeholder.Should().Be(placeholder);
        value.Name.Should().Be("Username");
    }

    [Fact]
    public void TextValue_Should_Change_Value()
    {
        var value = TextValue.Create(new EntryValueId(Guid.NewGuid()), "init", "Username", placeholder).Value;
        var result = value.SetValue("newValue");

        result.IsSuccess.Should().BeTrue();
        value.Value.Should().Be("newValue");
    }
}