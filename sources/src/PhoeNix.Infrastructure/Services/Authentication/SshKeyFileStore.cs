using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.Authentication;

public sealed class SshKeyFileStore : ISshKeyFileStore
{
    private const string CaFolderName = "ca";
    private const string SessionsFolderName = "sessions";
    private const string MachinesFolderName = "machines";

    private readonly string _rootPath;
    private readonly string _sshCaKeyName;
    private readonly string _deployCaKeyName;
    private readonly IProcessRunner _processRunner;

    public SshKeyFileStore(IAppSettingsRepository settingsRepository, IProcessRunner processRunner)
    {
        var settings = settingsRepository.GetFirstAsync().GetAwaiter().GetResult();

        _rootPath = settings is not null && !string.IsNullOrWhiteSpace(settings.FileStorageRootPath)
            ? settings.FileStorageRootPath
            : "/var/lib/phoenix";

        _sshCaKeyName = settings?.SshCaKeyName ?? "phoenix_user_ca";
        _deployCaKeyName = settings?.DeployCaKeyName ?? "phoenix-deploy-user-ca";
        _processRunner = processRunner;
    }

    public Result<string> GetOrCreateRootDirectory()
    {
        return CreateDirectoryIfMissing(_rootPath);
    }

    public Result<string> GetOrCreateCaDirectory()
    {
        return CreateDirectoryIfMissing(Path.Combine(_rootPath, CaFolderName));
    }

    public Result<string> GetOrCreateSessionDirectory(SetupSessionId sessionId)
    {
        return CreateDirectoryIfMissing(
            Path.Combine(_rootPath, SessionsFolderName, sessionId.Value.ToString()));
    }

    public Result<(string CaPrivateKeyPath, string CaPublicKeyPath)> GetCaKeyPaths()
    {
        return GetOrCreateCaDirectory()
            .Map(caDir =>
            {
                var priv = Path.Combine(caDir, _sshCaKeyName);
                var pub = priv + ".pub";
                return (priv, pub);
            });
    }

    public async Task<Result<string>> ReadCaPublicKeyAsync(CancellationToken cancellationToken)
    {
        var caPaths = GetCaKeyPaths();
        if (caPaths.IsFailure)
            return Result.Failure<string>(caPaths.Error);

        var publicKeyPath = caPaths.Value.CaPublicKeyPath;

        if (!File.Exists(publicKeyPath))
            return Result.Failure<string>(new Error(
                "SshCaPublicKeyMissing",
                $"SSH CA public key was not found at '{publicKeyPath}'."));

        try
        {
            var content = await File.ReadAllTextAsync(publicKeyPath, cancellationToken);
            var normalized = content.Trim();

            if (string.IsNullOrWhiteSpace(normalized))
                return Result.Failure<string>(new Error(
                    "SshCaPublicKeyEmpty",
                    $"SSH CA public key file '{publicKeyPath}' is empty."));

            return Result.Success(normalized);
        }
        catch (Exception e)
        {
            return Result.Failure<string>(new Error(
                "SshCaPublicKeyReadFailed",
                $"Failed to read SSH CA public key from '{publicKeyPath}': {e.Message}"));
        }
    }

    public Result<(string PrivateKeyPath, string PublicKeyPath, string CertificatePath)> GetSessionKeyPaths(
        SetupSessionId sessionId)
    {
        return GetOrCreateSessionDirectory(sessionId)
            .Map(sessionDir =>
            {
                var baseName = Path.Combine(sessionDir, "session_ed25519");
                var priv = baseName;
                var pub = baseName + ".pub";
                var cert = baseName + "-cert.pub";
                return (priv, pub, cert);
            });
    }

    public Result DeleteSessionDirectory(SetupSessionId sessionId)
    {
        var dir = Path.Combine(_rootPath, SessionsFolderName, sessionId.Value.ToString());

        try
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);

            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(new Error("SshKeyFileStoreDeleteFailed", e.Message));
        }
    }

    public Result EnsurePrivateKeyPermissions(string privateKeyPath)
    {
        var parent = Path.GetDirectoryName(privateKeyPath);
        if (!string.IsNullOrWhiteSpace(parent))
        {
            var dirResult = CreateDirectoryIfMissing(parent);
            if (dirResult.IsFailure)
                return Result.Failure(dirResult.Error with { Code = "SshKeyFileStoreCreateDirFailed" });
        }

        File.SetUnixFileMode(privateKeyPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        return Result.Success();
    }

    public Result<string> GetOrCreateMachineDirectory(MachineId machineId)
    {
        return CreateDirectoryIfMissing(
            Path.Combine(_rootPath, MachinesFolderName, machineId.Value.ToString()));
    }

    public Result<string> GetOrCreateMachineDeployDirectory(MachineId machineId)
    {
        return CreateDirectoryIfMissing(
            Path.Combine(_rootPath, MachinesFolderName, machineId.Value.ToString(), "deploy-ssh"));
    }

    public Result<(string CaPrivateKeyPath, string CaPublicKeyPath)> GetDeployCaKeyPaths()
    {
        return GetOrCreateCaDirectory()
            .Map(caDir =>
            {
                var priv = Path.Combine(caDir, _deployCaKeyName);
                var pub = priv + ".pub";
                return (priv, pub);
            });
    }

    public async Task<Result<string>> ReadDeployCaPublicKeyAsync(CancellationToken cancellationToken)
    {
        var caPaths = GetDeployCaKeyPaths();
        if (caPaths.IsFailure)
            return Result.Failure<string>(caPaths.Error);

        var publicKeyPath = caPaths.Value.CaPublicKeyPath;

        if (!File.Exists(publicKeyPath))
            return Result.Failure<string>(new Error(
                "DeploySshCaPublicKeyMissing",
                $"Deploy SSH CA public key was not found at '{publicKeyPath}'."));

        try
        {
            var content = await File.ReadAllTextAsync(publicKeyPath, cancellationToken);
            var normalized = content.Trim();

            if (string.IsNullOrWhiteSpace(normalized))
                return Result.Failure<string>(new Error(
                    "DeploySshCaPublicKeyEmpty",
                    $"Deploy SSH CA public key file '{publicKeyPath}' is empty."));

            return Result.Success(normalized);
        }
        catch (Exception e)
        {
            return Result.Failure<string>(new Error(
                "DeploySshCaPublicKeyReadFailed",
                $"Failed to read deploy SSH CA public key from '{publicKeyPath}': {e.Message}"));
        }
    }

    public Result<(string PrivateKeyPath, string PublicKeyPath, string CertificatePath)> GetMachineDeployKeyPaths(
        MachineId machineId)
    {
        return GetOrCreateMachineDeployDirectory(machineId)
            .Map(machineDir =>
            {
                var baseName = Path.Combine(machineDir, "deploy_ed25519");
                var priv = baseName;
                var pub = baseName + ".pub";
                var cert = baseName + "-cert.pub";
                return (priv, pub, cert);
            });
    }

    public Result DeleteMachineDeployDirectory(MachineId machineId)
    {
        var dir = Path.Combine(_rootPath, MachinesFolderName, machineId.Value.ToString(), "deploy-ssh");

        try
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);

            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(new Error("SshKeyFileStoreDeleteFailed", e.Message));
        }
    }

    private static Result<string> CreateDirectoryIfMissing(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
            return Result.Success(path);
        }
        catch (Exception e)
        {
            return Result.Failure<string>(new Error("SshKeyFileStoreCreateDirFailed", e.Message));
        }
    }
}
