using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Users;

public record EditUserModels(
    string Name,
    string Description,
    bool IsNormalUser,
    string HomePath,
    string Group,
    uint Uid,
    Shell Shell,
    List<string> ExtraGroups
);