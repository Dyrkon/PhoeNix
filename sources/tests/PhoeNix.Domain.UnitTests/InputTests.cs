using FluentAssertions;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;

namespace PhoeNix.Domain.UnitTests;

public class InputTests
{
    private readonly ConfigurationId _configurationId = new(new Guid("11111111-1111-1111-1111-111111111111"));

    private readonly InputId _inputId1 = new(Guid.NewGuid());
    private readonly InputId _inputId2 = new(Guid.NewGuid());

    private const string Nixpkgs = "github:NixOS/nixpkgs/nixos-unstable";
    private const string NixpkgsName = "nixpkgs";

    private const string Snowfall = "github:snowfallorg/lib";
    private const string SnowfallName = "snowfall";

    [Fact]
    public void Input_Should_Create_Input()
    {
        var result = Input.Create(_inputId1, _configurationId, Nixpkgs, NixpkgsName);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(_inputId1);
        result.Value.ConfigurationId.Should().Be(_configurationId);
        result.Value.Source.Should().Be(Nixpkgs);
        result.Value.Name.Should().Be(NixpkgsName);
        result.Value.Followers.Should().BeEmpty();
    }

    [Fact]
    public void Input_Should_Create_Input_That_Follows_Other_Input()
    {
        var nixpkgs = Input.Create(_inputId1, _configurationId, Nixpkgs, NixpkgsName).Value;

        var snowfall = Input.Create(_inputId2, _configurationId, Snowfall, SnowfallName, nixpkgs);

        snowfall.IsSuccess.Should().BeTrue();
        snowfall.Value.Name.Should().Be(SnowfallName);

        snowfall.Value.Followers.Should().ContainSingle(f =>
            f.InputId == _inputId2 &&
            f.FollowName == nixpkgs.Name &&
            f.FollowValue == nixpkgs.Name);
    }

    [Fact]
    public void Input_Should_Change_Source()
    {
        var input = Input.Create(_inputId1, _configurationId, Nixpkgs, NixpkgsName).Value;

        var result = input.ChangeSource(Snowfall);

        result.IsSuccess.Should().BeTrue();
        input.Source.Should().Be(Snowfall);
    }

    [Fact]
    public void Input_Should_Change_Name()
    {
        var input = Input.Create(_inputId1, _configurationId, Nixpkgs, NixpkgsName).Value;

        var result = input.ChangeName(SnowfallName);

        result.IsSuccess.Should().BeTrue();
        input.Name.Should().Be(SnowfallName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Input_Should_Fail_ChangeSource_When_Invalid(string? badSource)
    {
        var input = Input.Create(_inputId1, _configurationId, Nixpkgs, NixpkgsName).Value;

        var result = input.ChangeSource(badSource!);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Source can't be empty.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Input_Should_Fail_ChangeName_When_Invalid(string? badName)
    {
        var input = Input.Create(_inputId1, _configurationId, Nixpkgs, NixpkgsName).Value;

        var result = input.ChangeName(badName!);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Name can't be empty.");
    }

    [Fact]
    public void Input_Should_Add_Follow()
    {
        var input = Input.Create(_inputId1, _configurationId, Nixpkgs, NixpkgsName).Value;

        var result = input.AddFollow(SnowfallName, Snowfall);

        result.IsSuccess.Should().BeTrue();
        input.Followers.Should().ContainSingle(f =>
            f.InputId == _inputId1 &&
            f.FollowName == SnowfallName &&
            f.FollowValue == Snowfall);
    }

    [Fact]
    public void Input_Should_Not_Add_Duplicate_Follow_By_Name()
    {
        var input = Input.Create(_inputId1, _configurationId, Nixpkgs, NixpkgsName).Value;
        input.AddFollow(SnowfallName, Snowfall);

        var result = input.AddFollow(SnowfallName, "github:someone/else");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Inputs.FollowAlreadyExists");
        result.Error.Description.Should().Contain("already follows");
        input.Followers.Should().ContainSingle(f => f.FollowName == SnowfallName);
    }

    [Fact]
    public void Input_Should_Fail_AddFollow_When_Follows_Self()
    {
        var input = Input.Create(_inputId1, _configurationId, Nixpkgs, NixpkgsName).Value;

        var result = input.AddFollow(NixpkgsName, Nixpkgs);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Inputs.CannotFollowItself");
        result.Error.Description.Should().Be("Input can't follow itself.");
    }

    [Fact]
    public void Input_Should_Remove_Follow()
    {
        var input = Input.Create(_inputId1, _configurationId, Nixpkgs, NixpkgsName).Value;
        input.AddFollow(SnowfallName, Snowfall);

        var followId = input.Followers.Single().Id;

        var result = input.RemoveFollow(followId);

        result.IsSuccess.Should().BeTrue();
        input.Followers.Should().BeEmpty();
    }

    [Fact]
    public void Input_Should_Fail_RemoveFollow_When_Not_Found()
    {
        var input = Input.Create(_inputId1, _configurationId, Nixpkgs, NixpkgsName).Value;

        var result = input.RemoveFollow(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Inputs.FollowNotFound");
        result.Error.Description.Should().Contain("There is no follow");
    }
}
