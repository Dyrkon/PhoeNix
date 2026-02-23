using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Authentication;

public interface ISshKeyFileStore
{
    Result<string> GetOrCreateRootDirectory();
    Result<string> GetOrCreateCaDirectory();
    Result<string> GetOrCreateSessionDirectory(ProvisioningSessionId sessionId);

    Result<(string PrivateKeyPath, string PublicKeyPath, string CertificatePath)> GetSessionKeyPaths(
        ProvisioningSessionId sessionId);

    Result<(string CaPrivateKeyPath, string CaPublicKeyPath)> GetCaKeyPaths();

    Result DeleteSessionDirectory(ProvisioningSessionId sessionId);
    Result EnsurePrivateKeyPermissions(string privateKeyPath);
}