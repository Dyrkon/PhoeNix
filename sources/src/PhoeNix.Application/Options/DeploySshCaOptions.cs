namespace PhoeNix.Application.Options;

public sealed class DeploySshCaOptions
{
    public string KeyType { get; init; } = "ed25519";
    public string CaKeyName { get; init; } = "phoenix-deploy-user-ca";
    public string Principal { get; init; } = "phoenix-deploy";
    public string DeployUser { get; init; } = "phoenix-deploy";
    public TimeSpan CertificateTtl { get; init; } = TimeSpan.FromDays(365);
}