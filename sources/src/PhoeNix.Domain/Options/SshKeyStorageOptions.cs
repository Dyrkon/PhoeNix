namespace PhoeNix.Domain.Options;

public sealed class SshKeyStorageOptions
{
    public string RootPathName { get; init; } = ".phoenix";

    public string CaFolderName { get; init; } = "ca";
    public string SessionsFolderName { get; init; } = "sessions";
}
