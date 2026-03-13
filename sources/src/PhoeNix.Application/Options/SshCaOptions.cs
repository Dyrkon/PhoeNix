namespace PhoeNix.Application.Options;

public sealed class SshCaOptions
{
    public string CaKeyName { get; init; } = "phoenix_user_ca";

    public string Principal { get; init; } = "root";

    // Todo find sensible value
    public TimeSpan CertificateTtl { get; init; } = TimeSpan.FromHours(1);

    public string KeyType { get; init; } = "ed25519";
}