using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Models.SshIdentity;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.Authentication;

public sealed class SetupSshKeyProvider(
    ISshKeyFileStore fileStore,
    IProcessRunner processRunner,
    IAppSettingsRepository settingsRepository
) : ISetupSshKeyProvider
{
    public async Task<Result<SshIdentityMaterial>> GetOrCreateAsync(SetupSession session, CancellationToken ct)
    {
        var settings = await settingsRepository.GetAsync(session.OwnerId, ct);
        if (settings is null)
            return Result.Failure<SshIdentityMaterial>(new Error(
                "AppSettings.NotFound",
                "Application settings have not been initialized."));

        var nowUtc = DateTime.UtcNow;

        var rootDir = fileStore.GetOrCreateRootDirectory();
        if (rootDir.IsFailure) return Result.Failure<SshIdentityMaterial>(rootDir.Error);

        var caDir = fileStore.GetOrCreateCaDirectory();
        if (caDir.IsFailure) return Result.Failure<SshIdentityMaterial>(caDir.Error);

        var sessDir = fileStore.GetOrCreateSessionDirectory(session.Id);
        if (sessDir.IsFailure) return Result.Failure<SshIdentityMaterial>(sessDir.Error);

        var caPaths = fileStore.GetCaKeyPaths();
        if (caPaths.IsFailure) return Result.Failure<SshIdentityMaterial>(caPaths.Error);

        var ensureCa = EnsureCaKeypairExists(caPaths.Value.CaPrivateKeyPath, settings.SshCaKeyType, ct);
        if (ensureCa.IsFailure) return Result.Failure<SshIdentityMaterial>(ensureCa.Error);

        var sessionPaths = fileStore.GetSessionKeyPaths(session.Id);
        if (sessionPaths.IsFailure) return Result.Failure<SshIdentityMaterial>(sessionPaths.Error);

        var (privPath, pubPath, certPath) = sessionPaths.Value;

        var certificateTtl = TimeSpan.FromHours(settings.SshCaCertificateTtlHours);

        if (session.SshCredential is not null
            && session.SshCredential.IsValid(nowUtc)
            && File.Exists(privPath)
            && File.Exists(pubPath)
            && File.Exists(certPath))
            return Result.Success(new SshIdentityMaterial(
                privPath,
                pubPath,
                certPath,
                session.SshCredential.ExpiresAtUtc));

        if (session.SshCredential is not null && session.SshCredential.RevokedAtUtc is null &&
            !session.SshCredential.IsValid(nowUtc))
        {
            var revoke = session.RevokeSshCredential(nowUtc);
            if (revoke.IsFailure) return Result.Failure<SshIdentityMaterial>(revoke.Error);
        }

        var ensureSessionKey = EnsureSessionKeypairExists(privPath, settings.SshCaKeyType, ct);
        if (ensureSessionKey.IsFailure) return Result.Failure<SshIdentityMaterial>(ensureSessionKey.Error);

        var chmod = fileStore.EnsurePrivateKeyPermissions(privPath);
        if (chmod.IsFailure) return Result.Failure<SshIdentityMaterial>(chmod.Error);

        var expiresAtUtc = nowUtc.Add(certificateTtl);

        var sign = SignUserCertificate(
            caPaths.Value.CaPrivateKeyPath,
            pubPath,
            session.Id,
            expiresAtUtc,
            settings.SshCaPrincipal,
            certificateTtl,
            ct);

        if (sign.IsFailure) return Result.Failure<SshIdentityMaterial>(sign.Error);

        if (!File.Exists(certPath))
            return Result.Failure<SshIdentityMaterial>(new Error(
                "SshCertificateMissing",
                $"Expected certificate at '{certPath}' but it was not created."));

        var pubText = await File.ReadAllTextAsync(pubPath, ct);
        var certText = await File.ReadAllTextAsync(certPath, ct);

        var credential = new SshCredential(
            pubText,
            certText,
            expiresAtUtc,
            null);

        var assign = session.AssignSshCredential(credential, nowUtc);
        if (assign.IsFailure) return Result.Failure<SshIdentityMaterial>(assign.Error);

        return Result.Success(new SshIdentityMaterial(privPath, pubPath, certPath, expiresAtUtc));
    }

    public Task<Result> RevokeAsync(SetupSession session, CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;

        var revoke = session.RevokeSshCredential(nowUtc);
        if (revoke.IsFailure) return Task.FromResult(revoke);

        var delete = fileStore.DeleteSessionDirectory(session.Id);
        if (delete.IsFailure) return Task.FromResult(delete);

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
            "-C", "phoenix-user-ca"
        };

        var sshKeygenPath = Environment.GetEnvironmentVariable("PHOENIX_SSH_KEYGEN_PATH") ?? "ssh-keygen";
        var run = processRunner.RunProcess(sshKeygenPath, args, ct);
        if (run.IsFailure)
            return Result.Failure(run.Error with { Code = "SshCaKeygenFailed" });

        if (run.Value.ReturnCode != 0)
            return Result.Failure(new Error("SshCaKeygenFailed",
                $"ssh-keygen returned {run.Value.ReturnCode}: {run.Value.ErrorOutput}"));

        if (!File.Exists(caPrivateKeyPath) || !File.Exists(caPub))
            return Result.Failure(new Error("SshCaKeygenFailed", "CA keypair was not created as expected."));

        return Result.Success();
    }

    private Result EnsureSessionKeypairExists(string sessionPrivateKeyPath, string keyType, CancellationToken ct)
    {
        var pub = sessionPrivateKeyPath + ".pub";
        if (File.Exists(sessionPrivateKeyPath) && File.Exists(pub))
            return Result.Success();

        Directory.CreateDirectory(Path.GetDirectoryName(sessionPrivateKeyPath)!);

        var args = new List<string>
        {
            "-t", keyType,
            "-f", sessionPrivateKeyPath,
            "-N", "",
            "-C", "phoenix-session"
        };

        var sshKeygenPath = Environment.GetEnvironmentVariable("PHOENIX_SSH_KEYGEN_PATH") ?? "ssh-keygen";
        var run = processRunner.RunProcess(sshKeygenPath, args, ct);
        if (run.IsFailure)
            return Result.Failure(run.Error with { Code = "SshSessionKeygenFailed" });

        if (run.Value.ReturnCode != 0)
            return Result.Failure(new Error("SshSessionKeygenFailed",
                $"ssh-keygen returned {run.Value.ReturnCode}: {run.Value.ErrorOutput}"));

        if (!File.Exists(sessionPrivateKeyPath) || !File.Exists(pub))
            return Result.Failure(new Error("SshSessionKeygenFailed", "Session keypair was not created as expected."));

        return Result.Success();
    }

    private Result SignUserCertificate(
        string caPrivateKeyPath,
        string sessionPublicKeyPath,
        SetupSessionId sessionId,
        DateTime expiresAtUtc,
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
            "-I", $"phoenix-{sessionId.Value}",
            "-n", principal,
            "-V", validity,
            sessionPublicKeyPath
        };

        var sshKeygenPath = Environment.GetEnvironmentVariable("PHOENIX_SSH_KEYGEN_PATH") ?? "ssh-keygen";
        var run = processRunner.RunProcess(sshKeygenPath, args, ct);
        if (run.IsFailure)
            return Result.Failure(run.Error with { Code = "SshCertSignFailed" });

        if (run.Value.ReturnCode != 0)
            return Result.Failure(new Error("SshCertSignFailed",
                $"ssh-keygen returned {run.Value.ReturnCode}: {run.Value.ErrorOutput}"));

        return Result.Success();
    }
}
