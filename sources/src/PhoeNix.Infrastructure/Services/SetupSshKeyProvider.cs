using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Models;
using PhoeNix.Application.Models.SshIdentity;
using PhoeNix.Application.Options;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public sealed class SetupSshKeyProvider(
    ISshKeyFileStore fileStore,
    IProcessRunner processRunner,
    IOptions<SshCaOptions> caOptions
) : ISetupSshKeyProvider
{
    private readonly SshCaOptions _ca = caOptions.Value;

    public async Task<Result<SshIdentityMaterial>> GetOrCreateAsync(SetupSession session, CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;

        var rootDir = fileStore.GetOrCreateRootDirectory();
        if (rootDir.IsFailure) return Result.Failure<SshIdentityMaterial>(rootDir.Error);

        var caDir = fileStore.GetOrCreateCaDirectory();
        if (caDir.IsFailure) return Result.Failure<SshIdentityMaterial>(caDir.Error);

        var sessDir = fileStore.GetOrCreateSessionDirectory(session.Id);
        if (sessDir.IsFailure) return Result.Failure<SshIdentityMaterial>(sessDir.Error);

        var caPaths = fileStore.GetCaKeyPaths();
        if (caPaths.IsFailure) return Result.Failure<SshIdentityMaterial>(caPaths.Error);

        var ensureCa = EnsureCaKeypairExists(caPaths.Value.CaPrivateKeyPath, ct);
        if (ensureCa.IsFailure) return Result.Failure<SshIdentityMaterial>(ensureCa.Error);

        var sessionPaths = fileStore.GetSessionKeyPaths(session.Id);
        if (sessionPaths.IsFailure) return Result.Failure<SshIdentityMaterial>(sessionPaths.Error);

        var (privPath, pubPath, certPath) = sessionPaths.Value;

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

        var ensureSessionKey = EnsureSessionKeypairExists(privPath, ct);
        if (ensureSessionKey.IsFailure) return Result.Failure<SshIdentityMaterial>(ensureSessionKey.Error);

        var chmod = fileStore.EnsurePrivateKeyPermissions(privPath);
        if (chmod.IsFailure) return Result.Failure<SshIdentityMaterial>(chmod.Error);

        var expiresAtUtc = nowUtc.Add(_ca.CertificateTtl);

        var sign = SignUserCertificate(
            caPaths.Value.CaPrivateKeyPath,
            pubPath,
            session.Id,
            expiresAtUtc,
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
            "-C", "phoenix-user-ca"
        };

        var run = processRunner.RunProcess("ssh-keygen", args, ct);
        if (run.IsFailure)
            return Result.Failure(run.Error with { Code = "SshCaKeygenFailed" });

        if (run.Value.ReturnCode != 0)
            return Result.Failure(new Error("SshCaKeygenFailed",
                $"ssh-keygen returned {run.Value.ReturnCode}: {run.Value.ErrorOutput}"));

        if (!File.Exists(caPrivateKeyPath) || !File.Exists(caPub))
            return Result.Failure(new Error("SshCaKeygenFailed", "CA keypair was not created as expected."));

        return Result.Success();
    }

    private Result EnsureSessionKeypairExists(string sessionPrivateKeyPath, CancellationToken ct)
    {
        var pub = sessionPrivateKeyPath + ".pub";
        if (File.Exists(sessionPrivateKeyPath) && File.Exists(pub))
            return Result.Success();

        Directory.CreateDirectory(Path.GetDirectoryName(sessionPrivateKeyPath)!);

        var args = new List<string>
        {
            "-t", _ca.KeyType,
            "-f", sessionPrivateKeyPath,
            "-N", "",
            "-C", "phoenix-session"
        };

        var run = processRunner.RunProcess("ssh-keygen", args, ct);
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
        CancellationToken ct)
    {
        var validity = $"+{(int)_ca.CertificateTtl.TotalMinutes}m";
        if (_ca.CertificateTtl.TotalMinutes < 1)
            validity = $"+{(int)_ca.CertificateTtl.TotalSeconds}s";

        var args = new List<string>
        {
            "-s", caPrivateKeyPath,
            "-I", $"phoenix-{sessionId.Value}",
            "-n", _ca.Principal,
            "-V", validity,
            sessionPublicKeyPath
        };

        var run = processRunner.RunProcess("ssh-keygen", args, ct);
        if (run.IsFailure)
            return Result.Failure(run.Error with { Code = "SshCertSignFailed" });

        if (run.Value.ReturnCode != 0)
            return Result.Failure(new Error("SshCertSignFailed",
                $"ssh-keygen returned {run.Value.ReturnCode}: {run.Value.ErrorOutput}"));

        return Result.Success();
    }
}