using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Models.SshIdentity;
using PhoeNix.Application.Options;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public sealed class DeploySshKeyProvider(
    ISshKeyFileStore fileStore,
    IProcessRunner processRunner,
    IOptions<DeploySshCaOptions> caOptions)
    : IDeploySshKeyProvider
{
    private readonly DeploySshCaOptions _ca = caOptions.Value;

    public async Task<Result<DeploySshAccessMaterial>> GetOrCreateAsync(
        MachineId machineId,
        CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;

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

        var ensureCa = EnsureCaKeypairExists(caPaths.Value.CaPrivateKeyPath, ct);
        if (ensureCa.IsFailure)
            return Result.Failure<DeploySshAccessMaterial>(ensureCa.Error);

        var deployPaths = fileStore.GetMachineDeployKeyPaths(machineId);
        if (deployPaths.IsFailure)
            return Result.Failure<DeploySshAccessMaterial>(deployPaths.Error);

        var (privPath, pubPath, certPath) = deployPaths.Value;

        var ensureKey = EnsureDeployKeypairExists(privPath, ct);
        if (ensureKey.IsFailure)
            return Result.Failure<DeploySshAccessMaterial>(ensureKey.Error);

        var chmod = fileStore.EnsurePrivateKeyPermissions(privPath);
        if (chmod.IsFailure)
            return Result.Failure<DeploySshAccessMaterial>(chmod.Error);

        var sign = SignUserCertificate(
            caPaths.Value.CaPrivateKeyPath,
            pubPath,
            machineId,
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
            nowUtc.Add(_ca.CertificateTtl),
            _ca.DeployUser,
            _ca.Principal,
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

    private Result EnsureCaKeypairExists(string caPrivateKeyPath, CancellationToken ct)
    {
        var caPub = caPrivateKeyPath + ".pub";
        if (File.Exists(caPrivateKeyPath) && File.Exists(caPub))
            return Result.Success();

        Directory.CreateDirectory(Path.GetDirectoryName(caPrivateKeyPath)!);

        var args = new List<string>
        {
            "-t", _ca.KeyType,
            "-f", caPrivateKeyPath,
            "-N", "",
            "-C", "phoenix-deploy-user-ca"
        };

        var run = processRunner.RunProcess("ssh-keygen", args, ct);
        if (run.IsFailure)
            return Result.Failure(run.Error with { Code = "DeploySshCaKeygenFailed" });

        if (run.Value.ReturnCode != 0)
            return Result.Failure(new Error(
                "DeploySshCaKeygenFailed",
                $"ssh-keygen returned {run.Value.ReturnCode}: {run.Value.ErrorOutput}"));

        return Result.Success();
    }

    private Result EnsureDeployKeypairExists(string privateKeyPath, CancellationToken ct)
    {
        var pub = privateKeyPath + ".pub";
        if (File.Exists(privateKeyPath) && File.Exists(pub))
            return Result.Success();

        Directory.CreateDirectory(Path.GetDirectoryName(privateKeyPath)!);

        var args = new List<string>
        {
            "-t", _ca.KeyType,
            "-f", privateKeyPath,
            "-N", "",
            "-C", "phoenix-machine-deploy"
        };

        var run = processRunner.RunProcess("ssh-keygen", args, ct);
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
        CancellationToken ct)
    {
        var validity = $"+{(int)_ca.CertificateTtl.TotalMinutes}m";
        if (_ca.CertificateTtl.TotalMinutes < 1)
            validity = $"+{(int)_ca.CertificateTtl.TotalSeconds}s";

        var args = new List<string>
        {
            "-s", caPrivateKeyPath,
            "-I", $"phoenix-deploy-{machineId.Value}",
            "-n", _ca.Principal,
            "-V", validity,
            machinePublicKeyPath
        };

        var run = processRunner.RunProcess("ssh-keygen", args, ct);
        if (run.IsFailure)
            return Result.Failure(run.Error with { Code = "DeploySshCertSignFailed" });

        if (run.Value.ReturnCode != 0)
            return Result.Failure(new Error(
                "DeploySshCertSignFailed",
                $"ssh-keygen returned {run.Value.ReturnCode}: {run.Value.ErrorOutput}"));

        return Result.Success();
    }
}