namespace PhoeNix.Application.Options;

public sealed class SshKeyStorageOptions
{
    public string RootPath { get; set; } = "/var/lib/phoenix";
    public string CaFolderName { get; set; } = "ca";
    public string SessionsFolderName { get; set; } = "sessions";
}