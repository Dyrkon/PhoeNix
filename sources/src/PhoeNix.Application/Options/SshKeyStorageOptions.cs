namespace PhoeNix.Application.Options;

public sealed class SshKeyStorageOptions
{
    public string RootPath { get; init; } = "/var/lib/phoenix";
    public string CaFolderName { get; init; } = "ca";
    public string SessionsFolderName { get; init; } = "sessions";
    public string MachinesFolderName { get; init; } = "machines";
}