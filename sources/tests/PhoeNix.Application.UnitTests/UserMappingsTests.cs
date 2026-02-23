using FluentAssertions;
using PhoeNix.Application.Mappings;
using PhoeNix.Domain.Entities.SystemUsers;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.UnitTests;

public class UserMappingsTests
{
    [Fact]
    public void MapUserToDto_Should_Map_Correctly()
    {
        // Arrange
        var userId = new SystemUserId(Guid.NewGuid());
        var name = "Name";
        var description = "Description";
        var isNormalUser = true;
        var homePath = "./path";
        var group = "wheel";
        uint uid = 1024;
        var shell = Shell.Fish;
        var extraGroups = new List<string> { "one", "two" };
        var user = SystemUser.Create(userId, name, description, group, isNormalUser, uid, shell, homePath).Value;
        user.SetName(name);
        user.SetDescription(description);
        user.SetGroup(group);
        user.SetHomeLocation(homePath);
        user.SetShell(shell);
        user.SetNormalUserStatus(isNormalUser);
        user.SetUid(uid);
        foreach (var grp in extraGroups) user.AddExtraGroup(grp);

        // Act
        var result = UserMappings.MapUserToDto(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.Description.Should().Be(description);
        result.Name.Should().Be(name);
        result.Group.Should().Be(group);
        result.Shell.Should().Be(shell);
        result.HomePath.Should().Be(homePath);
        result.IsNormalUser.Should().Be(isNormalUser);
        result.ExtraGroups.Should().Equal(extraGroups);
    }
}