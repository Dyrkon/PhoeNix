namespace PhoeNix.Application.Models.SshIdentity;

public sealed record DeploySshAccessMaterial(
    string PrivateKeyPath,
    string PublicKeyPath,
    string CertificatePath,
    DateTime ExpiresAtUtc,
    string DeployUser,
    string Principal,
    string CaPublicKey);