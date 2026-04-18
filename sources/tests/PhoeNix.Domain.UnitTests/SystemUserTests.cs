using FluentAssertions;
using PhoeNix.Domain.Entities.SystemUsers;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.UnitTests;

public class SystemUserTests
{
    private readonly SystemUserId _id = new(Guid.NewGuid());

    private SystemUser CreateValid() =>
        SystemUser.Create(_id, "alice", "Alice Smith", "users", true, 1000, Shell.Bash, "/home/alice").Value;

    [Fact]
    public void SystemUser_Should_Create_Successfully()
    {
        var result = SystemUser.Create(_id, "alice", "Alice Smith", "users", true, 1000, Shell.Bash, "/home/alice");

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(_id);
        result.Value.Name.Should().Be("alice");
        result.Value.Description.Should().Be("Alice Smith");
        result.Value.Group.Should().Be("users");
        result.Value.IsNormalUser.Should().BeTrue();
        result.Value.Uid.Should().Be(1000u);
        result.Value.Shell.Should().Be(Shell.Bash);
        result.Value.HomePath.Should().Be("/home/alice");
        result.Value.ExtraGroups.Should().BeEmpty();
    }

    [Fact]
    public void SystemUser_Should_SetName()
    {
        var user = CreateValid();

        var result = user.SetName("bob");

        result.IsSuccess.Should().BeTrue();
        user.Name.Should().Be("bob");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SystemUser_Should_Fail_SetName_When_Empty(string name)
    {
        var user = CreateValid();

        var result = user.SetName(name);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Name cannot be empty.");
    }

    [Fact]
    public void SystemUser_Should_SetDescription()
    {
        var user = CreateValid();

        var result = user.SetDescription("Updated desc");

        result.IsSuccess.Should().BeTrue();
        user.Description.Should().Be("Updated desc");
    }

    [Fact]
    public void SystemUser_Should_SetNormalUserStatus()
    {
        var user = CreateValid();

        user.SetNormalUserStatus(false).IsSuccess.Should().BeTrue();
        user.IsNormalUser.Should().BeFalse();

        user.SetNormalUserStatus(true).IsSuccess.Should().BeTrue();
        user.IsNormalUser.Should().BeTrue();
    }

    [Fact]
    public void SystemUser_Should_SetHomeLocation()
    {
        var user = CreateValid();

        var result = user.SetHomeLocation("/home/newloc");

        result.IsSuccess.Should().BeTrue();
        user.HomePath.Should().Be("/home/newloc");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SystemUser_Should_Fail_SetHomeLocation_When_Empty(string location)
    {
        var user = CreateValid();

        var result = user.SetHomeLocation(location);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Home location cannot be empty.");
    }

    [Fact]
    public void SystemUser_Should_SetGroup()
    {
        var user = CreateValid();

        var result = user.SetGroup("wheel");

        result.IsSuccess.Should().BeTrue();
        user.Group.Should().Be("wheel");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SystemUser_Should_Fail_SetGroup_When_Empty(string group)
    {
        var user = CreateValid();

        var result = user.SetGroup(group);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Group cannot be empty.");
    }

    [Fact]
    public void SystemUser_Should_SetUid()
    {
        var user = CreateValid();

        var result = user.SetUid(500);

        result.IsSuccess.Should().BeTrue();
        user.Uid.Should().Be(500u);
    }

    [Theory]
    [InlineData(99u)]
    [InlineData(1000u)]
    public void SystemUser_Should_Fail_SetUid_When_Out_Of_Range(uint uid)
    {
        var user = CreateValid();

        var result = user.SetUid(uid);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("out of range");
    }

    [Fact]
    public void SystemUser_Should_SetShell()
    {
        var user = CreateValid();

        var result = user.SetShell(Shell.Fish);

        result.IsSuccess.Should().BeTrue();
        user.Shell.Should().Be(Shell.Fish);
    }

    [Fact]
    public void SystemUser_Should_AddExtraGroup()
    {
        var user = CreateValid();

        var result = user.AddExtraGroup("wheel");

        result.IsSuccess.Should().BeTrue();
        user.ExtraGroups.Should().ContainSingle("wheel");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SystemUser_Should_Fail_AddExtraGroup_When_Empty(string group)
    {
        var user = CreateValid();

        var result = user.AddExtraGroup(group);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Group name cannot be empty.");
    }

    [Fact]
    public void SystemUser_Should_Fail_AddExtraGroup_When_Duplicate()
    {
        var user = CreateValid();
        user.AddExtraGroup("wheel");

        var result = user.AddExtraGroup("wheel");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Group already exists.");
        user.ExtraGroups.Should().ContainSingle();
    }

    [Fact]
    public void SystemUser_Should_RemoveExtraGroup()
    {
        var user = CreateValid();
        user.AddExtraGroup("wheel");

        var result = user.RemoveExtraGroup("wheel");

        result.IsSuccess.Should().BeTrue();
        user.ExtraGroups.Should().BeEmpty();
    }

    [Fact]
    public void SystemUser_Should_Fail_RemoveExtraGroup_When_Not_Found()
    {
        var user = CreateValid();

        var result = user.RemoveExtraGroup("nonexistent");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Group not found.");
    }

    [Fact]
    public void SystemUser_Should_ClearExtraGroups()
    {
        var user = CreateValid();
        user.AddExtraGroup("wheel");
        user.AddExtraGroup("docker");

        var result = user.ClearExtraGroups();

        result.IsSuccess.Should().BeTrue();
        user.ExtraGroups.Should().BeEmpty();
    }

    [Fact]
    public void SystemUser_Should_Build_NixConfig()
    {
        var user = CreateValid();
        user.AddExtraGroup("wheel");

        var result = user.Build();

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Contain("alice");
        result.Value.Content.Should().Contain("Alice Smith");
        result.Value.Content.Should().Contain("users");
        result.Value.Content.Should().Contain("/home/alice");
        result.Value.Content.Should().Contain("wheel");
        result.Value.Shell.Should().Be(Shell.Bash);
    }
}
