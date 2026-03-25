namespace PhoeNix.Application.Options;

public sealed class NixosInstallerOptions
{
    public string ExecutableName { get; init; } = "nixos-anywhere";

    public string TargetUser { get; init; } = "root";

    public int InstallTimeoutMinutes { get; init; } = 90;

    public bool DisableHostKeyChecking { get; init; } = true;

    public bool BuildOnTarget { get; init; }

    public bool CopyHostKeys { get; init; }

    public List<string> ExtraArguments { get; init; } = [];
}