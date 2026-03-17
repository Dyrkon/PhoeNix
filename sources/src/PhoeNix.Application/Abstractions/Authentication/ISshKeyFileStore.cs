using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Authentication;

public interface ISshKeyFileStore
{
    Result<string> GetOrCreateRootDirectory();
    Result<string> GetOrCreateCaDirectory();
    Result<string> GetOrCreateSessionDirectory(SetupSessionId sessionId);

    Result<(string PrivateKeyPath, string PublicKeyPath, string CertificatePath)> GetSessionKeyPaths(
        SetupSessionId sessionId);

    Task<Result<string>> ReadCaPublicKeyAsync(CancellationToken cancellationToken);
    Result<(string CaPrivateKeyPath, string CaPublicKeyPath)> GetCaKeyPaths();

    Result DeleteSessionDirectory(SetupSessionId sessionId);
    Result EnsurePrivateKeyPermissions(string privateKeyPath);
}