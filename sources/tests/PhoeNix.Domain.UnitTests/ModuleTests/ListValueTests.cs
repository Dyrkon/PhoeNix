using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.UnitTests.ModuleTests;

public class ListValueTests
{
    private readonly string _placeholder = Guid.NewGuid().ToString();

    [Fact]
    public void ListValue_Should_Create_And_Retrieve_Items()
    {
        var items = new[] { "item1", "item2", "item3" };

        var result = ListValue.Create(new EntryValueId(Guid.NewGuid()), "Tags", _placeholder, items);

        result.IsSuccess.Should().BeTrue();
        result.Value.Kind.Should().Be(EntryValueKind.List);
        result.Value.Placeholder.Should().Be(_placeholder);
        result.Value.Name.Should().Be("Tags");
        result.Value.GetItems().Should().BeEquivalentTo(items);
    }

    [Fact]
    public void ListValue_Should_Create_With_Empty_Items()
    {
        var result = ListValue.Create(new EntryValueId(Guid.NewGuid()), "Tags", _placeholder, Array.Empty<string>());

        result.IsSuccess.Should().BeTrue();
        result.Value.GetItems().Should().BeEmpty();
    }

    [Fact]
    public void ListValue_Should_GetNixExpression_With_Items()
    {
        var items = new[] { "foo", "bar" };

        var value = ListValue.Create(new EntryValueId(Guid.NewGuid()), "Tags", _placeholder, items).Value;

        var nix = value.GetNixExpression();

        nix.Should().Be("[ foo bar ]");
    }

    [Fact]
    public void ListValue_Should_GetNixExpression_With_Empty_Items()
    {
        var value = ListValue.Create(new EntryValueId(Guid.NewGuid()), "Tags", _placeholder, Array.Empty<string>()).Value;

        var nix = value.GetNixExpression();

        nix.Should().Be("[ ]");
    }

    [Fact]
    public void ListValue_GetItems_Should_Return_Empty_When_Value_Is_Empty()
    {
        var value = ListValue.Create(new EntryValueId(Guid.NewGuid()), "Tags", _placeholder, Array.Empty<string>()).Value;
        value.Value = string.Empty;

        var items = value.GetItems();

        items.Should().BeEmpty();
    }
}
