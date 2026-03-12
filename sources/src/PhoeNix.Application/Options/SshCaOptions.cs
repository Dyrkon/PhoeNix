namespace PhoeNix.Application.Options;

public sealed class SshCaOptions
{
    public string CaKeyName { get; init; } = "phoenix_user_ca";

    public string Principal { get; init; } = "root";

    public TimeSpan CertificateTtl { get; init; } = TimeSpan.FromMinutes(15);

    public string KeyType { get; init; } = "ed25519";
}