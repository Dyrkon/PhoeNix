using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Models.SshIdentity;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.Authentication;

public sealed class DeploySshKeyProvider(
    ISshKeyFileStore fileStore,
    IProcessRunner processRunner,
    IAppSettingsRepository settingsRepository)
    : IDeploySshKeyProvider
{
    public async Task<Result<DeploySshAccessMaterial>> GetOrCreateAsync(
        MachineId machineId,
        CancellationToken ct)
    {
        var settings = await settingsRepository.GetFirstAsync(ct);
        if (settings is null)
            return Result.Failure<DeploySshAccessMaterial>(new Error(
                "AppSettings.NotFound",
                "Application settings have not been initialized."));

        var nowUtc = DateTime.UtcNow;
        var certificateTtl = TimeSpan.FromDays(settings.DeployCaCertificateTtlDays);

        var rootDir = fileStore.GetOrCreateRootDirectory();
        if (rootDir.IsFailure)
            return Result.Failure<DeploySshAccessMaterial>(rootDir.Error);

        var caDir = fileStore.GetOrCreateCaDirectory();
        if (caDir.IsFailure)
            return Result.Failure<DeploySshAccessMaterial>(caDir.Error);

        var machineDir = fileStore.GetOrCreateMachineDeployDirectory(machineId);
        if (machineDir.IsFailure)
            return Result.Failure<DeploySshAccessMaterial>(machineDir.Error);

        var caPaths = fileStore.GetDeployCaKeyPaths();
        if (caPaths.IsFailure)
            return Result.Failure<DeploySshAccessMaterial>(caPaths.Error);

        var ensureCa = EnsureCaKeypairExists(caPaths.Value.CaPrivateKeyPath, settings.DeployCaKeyType, ct);
        if (ensureCa.IsFailure)
            return Result.Failure<DeploySshAccessMaterial>(ensureCa.Error);

        var deployPaths = fileStore.GetMachineDeployKeyPaths(machineId);
        if (deployPaths.IsFailure)
            return Result.Failure<DeploySshAccessMaterial>(deployPaths.Error);

        var (privPath, pubPath, certPath) = deployPaths.Value;

        var ensureKey = EnsureDeployKeypairExists(privPath, settings.DeployCaKeyType, ct);
        if (ensureKey.IsFailure)
            return Result.Failure<DeploySshAccessMaterial>(ensureKey.Error);

        var chmod = fileStore.EnsurePrivateKeyPermissions(privPath);
        if (chmod.IsFailure)
            return Result.Failure<DeploySshAccessMaterial>(chmod.Error);

        var sign = SignUserCertificate(
            caPaths.Value.CaPrivateKeyPath,
            pubPath,
            machineId,
            settings.DeployCaPrincipal,
            certificateTtl,
            ct);

        if (sign.IsFailure)
            return Result.Failure<DeploySshAccessMaterial>(sign.Error);

        if (!File.Exists(certPath))
            return Result.Failure<DeploySshAccessMaterial>(new Error(
                "DeploySshCertificateMissing",
                $"Expected certificate at '{certPath}' but it was not created."));

        var caPublicKeyResult = await fileStore.ReadDeployCaPublicKeyAsync(ct);
        if (caPublicKeyResult.IsFailure)
            return Result.Failure<DeploySshAccessMaterial>(caPublicKeyResult.Error);

        return Result.Success(new DeploySshAccessMaterial(
            privPath,
            pubPath,
            certPath,
            nowUtc.Add(certificateTtl),
            settings.DeployCaDeployUser,
            settings.DeployCaPrincipal,
            caPublicKeyResult.Value));
    }

    public Task<Result> RevokeAsync(
        MachineId machineId,
        CancellationToken ct)
    {
        var delete = fileStore.DeleteMachineDeployDirectory(machineId);
        if (delete.IsFailure)
            return Task.FromResult(delete);

        return Task.FromResult(Result.Success());
    }

    private Result EnsureCaKeypairExists(string caPrivateKeyPath, string keyType, CancellationToken ct)
    {
        var caPub = caPrivateKeyPath + ".pub";
        if (File.Exists(caPrivateKeyPath) && File.Exists(caPub))
            return Result.Success();

        Directory.CreateDirectory(Path.GetDirectoryName(caPrivateKeyPath)!);

        var args = new List<string>
        {
            "-t", keyType,
            "-f", caPrivateKeyPath,
            "-N", "",
            "-C", "phoenix-deploy-user-ca"
        };

        var sshKeygenPath = Environment.GetEnvironmentVariable("PHOENIX_SSH_KEYGEN_PATH") ?? "ssh-keygen";
        var run = processRunner.RunProcess(sshKeygenPath, args, ct);
        if (run.IsFailure)
            return Result.Failure(run.Error with { Code = "DeploySshCaKeygenFailed" });

        if (run.Value.ReturnCode != 0)
            return Result.Failure(new Error(
                "DeploySshCaKeygenFailed",
                $"ssh-keygen returned {run.Value.ReturnCode}: {run.Value.ErrorOutput}"));

        return Result.Success();
    }

    private Result EnsureDeployKeypairExists(string privateKeyPath, string keyType, CancellationToken ct)
    {
        var pub = privateKeyPath + ".pub";
        if (File.Exists(privateKeyPath) && File.Exists(pub))
            return Result.Success();

        Directory.CreateDirectory(Path.GetDirectoryName(privateKeyPath)!);

        var args = new List<string>
        {
            "-t", keyType,
            "-f", privateKeyPath,
            "-N", "",
            "-C", "phoenix-machine-deploy"
        };

        var sshKeygenPath = Environment.GetEnvironmentVariable("PHOENIX_SSH_KEYGEN_PATH") ?? "ssh-keygen";

        var run = processRunner.RunProcess(sshKeygenPath, args, ct);
        if (run.IsFailure)
            return Result.Failure(run.Error with { Code = "DeploySshKeygenFailed" });

        if (run.Value.ReturnCode != 0)
            return Result.Failure(new Error(
                "DeploySshKeygenFailed",
                $"ssh-keygen returned {run.Value.ReturnCode}: {run.Value.ErrorOutput}"));

        return Result.Success();
    }

    private Result SignUserCertificate(
        string caPrivateKeyPath,
        string machinePublicKeyPath,
        MachineId machineId,
        string principal,
        TimeSpan certificateTtl,
        CancellationToken ct)
    {
        var validity = $"+{(int)certificateTtl.TotalMinutes}m";
        if (certificateTtl.TotalMinutes < 1)
            validity = $"+{(int)certificateTtl.TotalSeconds}s";

        var args = new List<string>
        {
            "-s", caPrivateKeyPath,
            "-I", $"phoenix-deploy-{machineId.Value}",
            "-n", principal,
            "-V", validity,
            machinePublicKeyPath
        };

        var sshKeygenPath = Environment.GetEnvironmentVariable("PHOENIX_SSH_KEYGEN_PATH") ?? "ssh-keygen";
        var run = processRunner.RunProcess(sshKeygenPath, args, ct);
        if (run.IsFailure)
            return Result.Failure(run.Error with { Code = "DeploySshCertSignFailed" });

        if (run.Value.ReturnCode != 0)
            return Result.Failure(new Error(
                "DeploySshCertSignFailed",
                $"ssh-keygen returned {run.Value.ReturnCode}: {run.Value.ErrorOutput}"));

        return Result.Success();
    }
}