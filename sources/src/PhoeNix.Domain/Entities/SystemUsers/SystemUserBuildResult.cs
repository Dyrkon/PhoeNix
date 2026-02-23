using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Entities.SystemUsers;

public record SystemUserBuildResult(string Content, Shell Shell, string ShellPlaceholder);