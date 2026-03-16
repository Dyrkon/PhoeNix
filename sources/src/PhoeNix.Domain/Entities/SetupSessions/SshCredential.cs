namespace PhoeNix.Domain.Entities.SetupSessions;

public record SshCredential(
    string PublicKey,
    string CertificatePublicKey,
    DateTime ExpiresAtUtc,
    DateTime? RevokedAtUtc
);

public static class SshCredentialExtensions
{
    public static bool IsValid(this SshCredential credential, DateTime nowUtc)
    {
        return credential.RevokedAtUtc is null && credential.ExpiresAtUtc > nowUtc;
    }
}