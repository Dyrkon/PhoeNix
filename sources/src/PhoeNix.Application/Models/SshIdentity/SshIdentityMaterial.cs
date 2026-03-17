namespace PhoeNix.Application.Models.SshIdentity;

public record SshIdentityMaterial(
    string PrivateKeyPath,
    string PublicKeyPath,
    string CertificatePath,
    DateTime ExpiresAtUtc
);