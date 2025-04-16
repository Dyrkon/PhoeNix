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
        var followsId = new InputId(Guid.NewGuid());

        var input = Input.Create(inputId, "github:nixos", "nixpkgs", followsId).Value;

        var result = InputMappings.MapInputToDto(input);

        result.Should().NotBeNull();
        result.Id.Should().Be(input.Id);
        result.Source.Should().Be(input.Source);
        result.Name.Should().Be(input.Name);
        result.Follows.Should().Be(followsId);
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