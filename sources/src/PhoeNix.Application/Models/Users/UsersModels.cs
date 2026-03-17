using PhoeNix.Domain.Entities.SystemUsers;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Users;

public record UserListResponse(SystemUserId Id, string Name, string Description);

public record UserResponse(
    SystemUserId Id,
    string Name,
    string Description,
    bool IsNormalUser,
    string HomePath,
    string Group,
    uint Uid,
    Shell Shell,
    List<string> ExtraGroups
);