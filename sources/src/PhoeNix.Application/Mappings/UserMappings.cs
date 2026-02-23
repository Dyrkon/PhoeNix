using PhoeNix.Application.Models.Users;
using PhoeNix.Domain.Entities.SystemUsers;

namespace PhoeNix.Application.Mappings;

public static class UserMappings
{
    public static UserResponse MapUserToDto(SystemUser systemUser)
    {
        return new UserResponse(systemUser.Id, systemUser.Name, systemUser.Description, systemUser.IsNormalUser,
            systemUser.HomePath,
            systemUser.Group, systemUser.Uid, systemUser.Shell, systemUser.ExtraGroups.ToList());
    }
}