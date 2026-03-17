using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Options;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public sealed class SshKeyFileStore : ISshKeyFileStore
{
    private SshKeyStorageOptions Storage => _storageOptions.Value;
    private SshCaOptions Ca => _caOptions.Value;

    private readonly string _rootPath;
    private readonly IOptions<SshKeyStorageOptions> _storageOptions;
    private readonly IOptions<SshCaOptions> _caOptions;
    private readonly IProcessRunner _processRunner;

    public SshKeyFileStore(
        IOptions<SshKeyStorageOptions> storageOptions,
        IOptions<SshCaOptions> caOptions,
        IProcessRunner processRunner)
    {
        _storageOptions = storageOptions;
        _caOptions = caOptions;
        _processRunner = processRunner;

        _rootPath = string.IsNullOrWhiteSpace(Storage.RootPath)
            ? "/var/lib/phoenix"
            : Storage.RootPath;
    }

    public Result<string> GetOrCreateRootDirectory()
    {
        return CreateDirectoryIfMissing(_rootPath);
    }

    public Result<string> GetOrCreateCaDirectory()
    {
        return CreateDirectoryIfMissing(Path.Combine(_rootPath, Storage.CaFolderName));
    }

    public Result<string> GetOrCreateSessionDirectory(SetupSessionId sessionId)
    {
        return CreateDirectoryIfMissing(
            Path.Combine(_rootPath, Storage.SessionsFolderName, sessionId.Value.ToString()));
    }

    public Result<(string CaPrivateKeyPath, string CaPublicKeyPath)> GetCaKeyPaths()
    {
        return GetOrCreateCaDirectory()
            .Map(caDir =>
            {
                var priv = Path.Combine(caDir, Ca.CaKeyName);
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
        var dir = Path.Combine(_rootPath, Storage.SessionsFolderName, sessionId.Value.ToString());

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

        var result = _processRunner.RunProcess(
            "chmod",
            ["600", privateKeyPath],
            CancellationToken.None);

        if (result.IsFailure)
            return Result.Failure(result.Error with { Code = "SshKeyFileStoreChmodFailed" });

        if (result.Value.ReturnCode != 0)
            return Result.Failure(new Error(
                "SshKeyFileStoreChmodFailed",
                $"chmod returned {result.Value.ReturnCode}: {result.Value.ErrorOutput}"
            ));

        return Result.Success();
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