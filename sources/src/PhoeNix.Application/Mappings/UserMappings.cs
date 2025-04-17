using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Models.Users;

namespace PhoeNix.Application.Mappings;

public static class UserMappings
{
    public static UserResponse MapUserToDto(User user)
    {
        return new UserResponse(user.Id);
    }
}