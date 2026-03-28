using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Authentication;

public interface ISshKeyFileStore
{
    Result<string> GetOrCreateRootDirectory();
    Result<string> GetOrCreateCaDirectory();
    Result<string> GetOrCreateSessionDirectory(SetupSessionId sessionId);

    Result<string> GetOrCreateMachineDirectory(MachineId machineId);
    Result<string> GetOrCreateMachineDeployDirectory(MachineId machineId);

    Result<(string CaPrivateKeyPath, string CaPublicKeyPath)> GetCaKeyPaths();
    Result<(string CaPrivateKeyPath, string CaPublicKeyPath)> GetDeployCaKeyPaths();

    Task<Result<string>> ReadCaPublicKeyAsync(CancellationToken cancellationToken);
    Task<Result<string>> ReadDeployCaPublicKeyAsync(CancellationToken cancellationToken);

    Result<(string PrivateKeyPath, string PublicKeyPath, string CertificatePath)> GetSessionKeyPaths(
        SetupSessionId sessionId);

    Result<(string PrivateKeyPath, string PublicKeyPath, string CertificatePath)> GetMachineDeployKeyPaths(
        MachineId machineId);

    Result DeleteSessionDirectory(SetupSessionId sessionId);
    Result DeleteMachineDeployDirectory(MachineId machineId);

    Result EnsurePrivateKeyPermissions(string privateKeyPath);
}