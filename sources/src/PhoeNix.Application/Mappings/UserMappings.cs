using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Models.Users;

namespace PhoeNix.Application.Mappings;

public static class UserMappings
{
    public static UserResponse MapUserToDto(User user)
    {
        return new UserResponse(user.Id, user.Name, user.Description, user.IsNormalUser, user.HomePath, user.Group, user.Uid, user.Shell, user.ExtraGroups.ToList());
    }
}