using FluentAssertions;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Extensions;

namespace PhoeNix.Domain.UnitTests;

public class InputTests
{
    private readonly InputId InputId1 = new(Guid.NewGuid());
    private readonly InputId InputId2 = new(Guid.NewGuid());
    private readonly InputId InputId3 = new(Guid.NewGuid());
    private readonly string Nixpkgs = "github:NixOS/nixpkgs/nixos-unstable";
    private readonly string NixpkgsName = "nixpkgs";
    private readonly string Snowfall = "github:snowfallorg/lib";
    private readonly string SnowfallName = "snowfall";

    [Fact]
    public void Input_Should_Create_Input()
    {
        var input = Input.Create(InputId1, Nixpkgs, NixpkgsName);

        input.IsSuccess.Should().BeTrue();
        input.Value.Name.Should().Be(NixpkgsName);
    }

    [Fact]
    public void Input_Should_Create_Input_That_Follows()
    {
        var snowfall = Input.Create(InputId1, Nixpkgs, NixpkgsName)
            .Bind(r => Input.Create(InputId2, Snowfall, SnowfallName, r.Id));

        snowfall.IsSuccess.Should().BeTrue();
        snowfall.Value.Name.Should().Be(SnowfallName);
        snowfall.Value.Follows.Should().Be(InputId1);
    }

    [Fact]
    public void Input_Should_Change_Source()
    {
        var input = Input.Create(InputId1, Nixpkgs, NixpkgsName);

        var result = input.Value.ChangeSource(Snowfall);

        result.IsSuccess.Should().BeTrue();
        input.Value.Source.Should().Be(Snowfall);
    }

    [Fact]
    public void Input_Should_Change_Name()
    {
        var input = Input.Create(InputId1, Nixpkgs, NixpkgsName);

        var result = input.Value.ChangeName(SnowfallName);

        result.IsSuccess.Should().BeTrue();
        input.Value.Name.Should().Be(SnowfallName);
    }

    [Fact]
    public void Input_Should_Change_Following_Input()
    {
        var input = Input.Create(InputId1, Nixpkgs, NixpkgsName, InputId2);

        var result = input.Value.ChangeFollows(InputId3);

        result.IsSuccess.Should().BeTrue();
        input.Value.Follows.Should().Be(InputId3);
    }

    [Theory]
    [InlineData("")]
    public void Input_Should_Fail_ChangeSource_When_Invalid(string badSource)
    {
        var input = Input.Create(InputId1, Nixpkgs, NixpkgsName);

        var result = input.Value.ChangeSource(badSource);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Source can't be empty");
    }

    [Theory]
    [InlineData("")]
    public void Input_Should_Fail_ChangeName_When_Invalid(string badName)
    {
        var input = Input.Create(InputId1, Nixpkgs, NixpkgsName);

        var result = input.Value.ChangeName(badName);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Name can't be empty");
    }

    [Fact]
    public void Input_Should_Fail_ChangeFollows_When_Follows_Self()
    {
        var input = Input.Create(InputId1, Nixpkgs, NixpkgsName);

        var result = input.Value.ChangeFollows(InputId1);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Input can't follow itself");
    }

    [Fact]
    public void Input_Should_Fail_ChangeFollows_When_Following_Same_Input()
    {
        var input = Input.Create(InputId1, Nixpkgs, NixpkgsName, InputId2);

        var result = input.Value.ChangeFollows(InputId2);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be($"This input already follows this input ({InputId2})");
    }
}