namespace PhoeNix.Domain.Entities.VmHosts;

public sealed record VmHostCredential
{
    private VmHostCredential()
    {
    }

    public string Host { get; private set; } = default!;

    public int? Port { get; private set; }

    public string? Username { get; private set; }

    public string? Secret { get; private set; }

    public string? ExtraConfig { get; private set; }

    public static VmHostCredential Create(
        string host,
        int? port,
        string? username,
        string? secret,
        string? extraConfig)
    {
        return new VmHostCredential
        {
            Host = host,
            Port = port,
            Username = username,
            Secret = secret,
            ExtraConfig = extraConfig
        };
    }
}
