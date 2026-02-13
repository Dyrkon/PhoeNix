using FluentAssertions;
using PhoeNix.Application.Mappings;
using PhoeNix.Domain.Entities.Inputs;

namespace PhoeNix.Application.UnitTests;

public class InputMappingsTests
{
    [Fact]
    public void MapInputToDto_Should_Map_Correctly()
    {
        var inputId = new InputId(Guid.NewGuid());
        var follows = Input.Create(inputId, "github:foo", "foo").Value;

        var input = Input.Create(inputId, "github:nixos", "nixpkgs", follows).Value;

        var result = InputMappings.MapInputToDto(input);

        result.Should().NotBeNull();
        result.Id.Should().Be(input.TemplateId);
        result.Source.Should().Be(input.Source);
        result.Name.Should().Be(input.Name);
        result.Follows.Id.Should().Be(follows.TemplateId);
    }

    [Fact]
    public void MapInputToDto_Should_Handle_Null_Follows()
    {
        var inputId = new InputId(Guid.NewGuid());
        var input = Input.Create(inputId, "github:nixos", "nixpkgs").Value;

        var result = InputMappings.MapInputToDto(input);

        result.Should().NotBeNull();
        result.Follows.Should().BeNull();
    }
}