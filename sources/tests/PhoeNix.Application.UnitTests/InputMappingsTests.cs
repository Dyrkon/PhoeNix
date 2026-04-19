using FluentAssertions;
using PhoeNix.Application.Mappings;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;

namespace PhoeNix.Application.UnitTests;

public class InputMappingsTests
{
    private readonly ConfigurationId _configId = new(Guid.NewGuid());

    [Fact]
    public void MapInputToDto_Should_Map_Correctly_When_No_Follows()
    {
        var input = Input.Create(new InputId(Guid.NewGuid()), _configId, "github:nixos", "nixpkgs").Value;

        var dto = InputMappings.MapInputToDto(input);

        dto.Id.Should().Be(input.Id.Value);
        dto.Source.Should().Be("github:nixos");
        dto.Name.Should().Be("nixpkgs");
        dto.Followers.Should().BeEmpty();
    }

    [Fact]
    public void MapInputToDto_Should_Map_Follows()
    {
        var input = Input.Create(new InputId(Guid.NewGuid()), _configId, "github:nixos", "nixpkgs").Value;
        input.AddFollow("flake-utils", "github:numtide/flake-utils").IsSuccess.Should().BeTrue();

        var dto = InputMappings.MapInputToDto(input);

        dto.Followers.Should().ContainSingle(f =>
            f.FollowName == "flake-utils" &&
            f.FollowValue == "github:numtide/flake-utils");
    }
}