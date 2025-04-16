using FluentAssertions;
using PhoeNix.Application.Mappings;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Application.UnitTests;

public class UserMappingsTests
{
    [Fact]
    public void MapUserToDto_Should_Map_Correctly()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());
        var user = User.Create(userId).Value;

        // Act
        var result = UserMappings.MapUserToDto(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
    }
}