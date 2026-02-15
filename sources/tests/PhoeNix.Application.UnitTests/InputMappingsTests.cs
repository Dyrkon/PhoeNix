using FluentAssertions;
using PhoeNix.Application.Mappings;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;

namespace PhoeNix.Application.UnitTests;

public class InputMappingsTests
{
    private readonly ConfigurationId _configurationId = new(Guid.NewGuid());

    [Fact]
    public void MapInputToDto_Should_Map_Correctly_When_No_Followers()
    {
        var inputId = new InputId(Guid.NewGuid());
        var input = Input.Create(inputId, _configurationId, "github:nixos", "nixpkgs").Value;

        var dto = InputMappings.MapInputToDto(input);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(input.Id);
        dto.Source.Should().Be(input.Source);
        dto.Name.Should().Be(input.Name);
        dto.FollowInputs.Should().NotBeNull();
        dto.FollowInputs.Should().BeEmpty();
    }

    [Fact]
    public void MapInputToDto_Should_Map_Correctly_When_Has_Followers()
    {
        var follows = Input.Create(new InputId(Guid.NewGuid()), _configurationId, "github:foo", "foo").Value;

        var input = Input.Create(
            new InputId(Guid.NewGuid()),
            _configurationId,
            "github:nixos",
            "nixpkgs",
            follows).Value;

        var dto = InputMappings.MapInputToDto(input);

        dto.Should().NotBeNull();
        dto.Id.Should().Be(input.Id);
        dto.Source.Should().Be(input.Source);
        dto.Name.Should().Be(input.Name);

        dto.FollowInputs.Should().ContainSingle(f =>
            f.FollowName == follows.Name &&
            f.FollowValue == follows.Name);
    }

    [Fact]
    public void MapInputsFollowsToDto_Should_Map_List()
    {
        var input = Input.Create(new InputId(Guid.NewGuid()), _configurationId, "github:nixos", "nixpkgs").Value;
        input.AddFollow("flake-utils", "github:numtide/flake-utils");

        var dtoList = InputMappings.MapInputsFollowsToDto(input.Followers.ToList());

        dtoList.Should().ContainSingle(f =>
            f.FollowName == "flake-utils" &&
            f.FollowValue == "github:numtide/flake-utils");
    }
}