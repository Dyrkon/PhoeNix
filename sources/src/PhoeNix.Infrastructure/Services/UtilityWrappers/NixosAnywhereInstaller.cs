using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Abstractions.Setup;
using PhoeNix.Application.Options;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.UtilityWrappers;

public sealed class NixosAnywhereInstaller(
    IProcessRunner processRunner,
    ISshKeyFileStore sshKeyFileStore,
    IOptions<NixosInstallerOptions> nixosAnywhereOptions)
    : INixosInstaller
{
    private readonly NixosInstallerOptions _options = nixosAnywhereOptions.Value;

    public Task<Result> InstallAsync(
        SetupSession session,
        SetupTarget target,
        string configurationDirectoryPath,
        string configurationName,
        CancellationToken cancellationToken)
    {
        if (session.SshCredential is null)
            return Task.FromResult(Result.Failure(new Error(
                "NixosAnywhereSshCredentialMissing",
                "The setup session does not have an SSH credential assigned.")));

        if (!session.SshCredential.IsValid(DateTime.UtcNow))
            return Task.FromResult(Result.Failure(new Error(
                "NixosAnywhereSshCredentialInvalid",
                "The setup session SSH credential is expired or revoked.")));

        if (target.IpAddress is null)
            return Task.FromResult(Result.Failure(new Error(
                "NixosAnywhereTargetIpMissing",
                "The setup target does not have a recorded IP address.")));

        if (string.IsNullOrWhiteSpace(configurationDirectoryPath))
            return Task.FromResult(Result.Failure(new Error(
                "NixosAnywhereConfigurationPathMissing",
                "The configuration directory path cannot be empty.")));

        if (!Directory.Exists(configurationDirectoryPath))
            return Task.FromResult(Result.Failure(new Error(
                "NixosAnywhereConfigurationPathNotFound",
                $"The configuration directory '{configurationDirectoryPath}' does not exist.")));

        var keyPathsResult = sshKeyFileStore.GetSessionKeyPaths(session.Id);
        if (keyPathsResult.IsFailure)
            return Task.FromResult<Result>(keyPathsResult);

        var keyPaths = keyPathsResult.Value;

        if (!File.Exists(keyPaths.PrivateKeyPath))
            return Task.FromResult(Result.Failure(new Error(
                "NixosAnywherePrivateKeyMissing",
                $"The SSH private key '{keyPaths.PrivateKeyPath}' was not found.")));

        if (!File.Exists(keyPaths.CertificatePath))
            return Task.FromResult(Result.Failure(new Error(
                "NixosAnywhereCertificateMissing",
                $"The SSH certificate '{keyPaths.CertificatePath}' was not found.")));

        var chmodResult = sshKeyFileStore.EnsurePrivateKeyPermissions(keyPaths.PrivateKeyPath);
        if (chmodResult.IsFailure)
            return Task.FromResult(chmodResult);

        var arguments = BuildArguments(
            configurationDirectoryPath,
            target.IpAddress.ToString(),
            configurationName,
            keyPaths.PrivateKeyPath,
            keyPaths.CertificatePath);

        var processResult = processRunner.RunProcess(
            _options.ExecutableName,
            arguments,
            cancellationToken,
            workingDirectory: configurationDirectoryPath,
            timeOut: TimeSpan.FromMinutes(_options.InstallTimeoutMinutes));

        if (processResult.IsFailure)
            return Task.FromResult(Result.Failure(processResult.Error with { Code = "NixosAnywhereInstallFailed" }));

        return Task.FromResult(Result.Success());
    }

    private List<string> BuildArguments(
        string configurationDirectoryPath,
        string ipAddress,
        string configurationName,
        string privateKeyPath,
        string certificatePath)
    {
        var arguments = new List<string>
        {
            "--flake",
            $"{configurationDirectoryPath}#{configurationName}"
        };

        if (_options.BuildOnTarget)
            arguments.Add("--build-on-remote");

        if (_options.CopyHostKeys)
            arguments.Add("--copy-host-keys");

        arguments.Add("--target-host");
        arguments.Add($"{_options.TargetUser}@{ipAddress}");

        arguments.Add("--ssh-option");
        arguments.Add($"IdentityFile={privateKeyPath}");

        arguments.Add("--ssh-option");
        arguments.Add($"CertificateFile={certificatePath}");

        arguments.Add("--ssh-option");
        arguments.Add("BatchMode=yes");

        if (_options.DisableHostKeyChecking)
        {
            arguments.Add("--ssh-option");
            arguments.Add("StrictHostKeyChecking=no");

            arguments.Add("--ssh-option");
            arguments.Add("UserKnownHostsFile=/dev/null");
        }

        if (_options.ExtraArguments.Count > 0)
            arguments.AddRange(_options.ExtraArguments);

        return arguments;
    }
}