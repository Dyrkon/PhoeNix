using FluentAssertions;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Domain.UnitTests;

public class UserTests
{
    private readonly UserId _userId = new(Guid.NewGuid());

    [Fact]
    public void User_Should_Create_Successfully()
    {
        var result = User.Create(_userId, "alice");

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(_userId);
        result.Value.Name.Should().Be("alice");
        result.Value.NormalizedName.Should().Be("ALICE");
        result.Value.PasswordHash.Should().BeEmpty();
        result.Value.UserSshKeys.Should().BeEmpty();
    }

    [Fact]
    public void User_Should_Trim_Name_On_Create()
    {
        var result = User.Create(_userId, "  bob  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("bob");
        result.Value.NormalizedName.Should().Be("BOB");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void User_Should_Fail_Create_When_Name_Empty(string name)
    {
        var result = User.Create(_userId, name);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("User name is required.");
    }

    [Fact]
    public void User_NormalizeName_Should_Uppercase_And_Trim()
    {
        var normalized = User.NormalizeName("  Alice  ");

        normalized.Should().Be("ALICE");
    }

    [Fact]
    public void User_Should_SetPasswordHash()
    {
        var user = User.Create(_userId, "alice").Value;

        var result = user.SetPasswordHash("$6$hash$abc");

        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be("$6$hash$abc");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void User_Should_Fail_SetPasswordHash_When_Empty(string hash)
    {
        var user = User.Create(_userId, "alice").Value;

        var result = user.SetPasswordHash(hash);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Password hash is required.");
    }

    [Fact]
    public void User_Should_AddSshKey()
    {
        var user = User.Create(_userId, "alice").Value;

        var result = user.AddSshKey("ssh-ed25519 AAAAC3... alice@host");

        result.IsSuccess.Should().BeTrue();
        user.UserSshKeys.Should().ContainSingle();
    }

    [Fact]
    public void User_Should_Trim_SshKey_On_Add()
    {
        var user = User.Create(_userId, "alice").Value;

        user.AddSshKey("  ssh-ed25519 key  ");

        user.UserSshKeys.Should().ContainSingle().Which.Should().Be("ssh-ed25519 key");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void User_Should_Fail_AddSshKey_When_Empty(string key)
    {
        var user = User.Create(_userId, "alice").Value;

        var result = user.AddSshKey(key);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("SSH key is required.");
    }

    [Fact]
    public void User_Should_Fail_AddSshKey_When_Duplicate()
    {
        var user = User.Create(_userId, "alice").Value;
        user.AddSshKey("ssh-ed25519 key");

        var result = user.AddSshKey("ssh-ed25519 key");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already exists");
        user.UserSshKeys.Should().ContainSingle();
    }

    [Fact]
    public void User_Should_RemoveSshKey()
    {
        var user = User.Create(_userId, "alice").Value;
        user.AddSshKey("ssh-ed25519 key");

        var result = user.RemoveSshKey("ssh-ed25519 key");

        result.IsSuccess.Should().BeTrue();
        user.UserSshKeys.Should().BeEmpty();
    }

    [Fact]
    public void User_Should_Fail_RemoveSshKey_When_Not_Found()
    {
        var user = User.Create(_userId, "alice").Value;

        var result = user.RemoveSshKey("ssh-ed25519 nonexistent");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("does not exist");
    }
}
