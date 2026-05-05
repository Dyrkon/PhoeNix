using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Abstractions.Setup;
using PhoeNix.Application.Options;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.UtilityWrappers;

public sealed class NixosAnywhereInstaller(
    IProcessRunner processRunner,
    ISshKeyFileStore sshKeyFileStore,
    IOptions<NixosInstallerOptions> nixosAnywhereOptions,
    IAppSettingsRepository settingsRepository)
    : INixosInstaller
{
    public async Task<Result> InstallAsync(
        SetupSession session,
        SetupTarget target,
        string configurationDirectoryPath,
        string configurationName,
        CancellationToken cancellationToken)
    {
        var settings = await settingsRepository.GetAsync(session.OwnerId, cancellationToken);
        if (settings is null)
            return Result.Failure(new Error(
                "AppSettings.NotFound",
                "Application settings have not been initialized."));

        if (session.SshCredential is null)
            return Result.Failure(new Error(
                "NixosAnywhereSshCredentialMissing",
                "The setup session does not have an SSH credential assigned."));

        if (!session.SshCredential.IsValid(DateTime.UtcNow))
            return Result.Failure(new Error(
                "NixosAnywhereSshCredentialInvalid",
                "The setup session SSH credential is expired or revoked."));

        if (target.IpAddress is null)
            return Result.Failure(new Error(
                "NixosAnywhereTargetIpMissing",
                "The setup target does not have a recorded IP address."));

        if (string.IsNullOrWhiteSpace(configurationDirectoryPath))
            return Result.Failure(new Error(
                "NixosAnywhereConfigurationPathMissing",
                "The configuration directory path cannot be empty."));

        if (!Directory.Exists(configurationDirectoryPath))
            return Result.Failure(new Error(
                "NixosAnywhereConfigurationPathNotFound",
                $"The configuration directory '{configurationDirectoryPath}' does not exist."));

        var keyPathsResult = sshKeyFileStore.GetSessionKeyPaths(session.Id);
        if (keyPathsResult.IsFailure)
            return keyPathsResult;

        var keyPaths = keyPathsResult.Value;

        if (!File.Exists(keyPaths.PrivateKeyPath))
            return Result.Failure(new Error(
                "NixosAnywherePrivateKeyMissing",
                $"The SSH private key '{keyPaths.PrivateKeyPath}' was not found."));

        if (!File.Exists(keyPaths.CertificatePath))
            return Result.Failure(new Error(
                "NixosAnywhereCertificateMissing",
                $"The SSH certificate '{keyPaths.CertificatePath}' was not found."));

        var chmodResult = sshKeyFileStore.EnsurePrivateKeyPermissions(keyPaths.PrivateKeyPath);
        if (chmodResult.IsFailure)
            return chmodResult;

        var arguments = BuildArguments(
            configurationDirectoryPath,
            target.IpAddress.ToString(),
            configurationName,
            keyPaths.PrivateKeyPath,
            keyPaths.CertificatePath,
            settings.InstallerTargetUser,
            settings.InstallerBuildOnTarget,
            settings.InstallerCopyHostKeys,
            settings.InstallerDisableHostKeyChecking,
            nixosAnywhereOptions.Value.ExtraArguments);

        var processResult = processRunner.RunProcess(
            settings.InstallerExecutableName,
            arguments,
            cancellationToken,
            timeOut: TimeSpan.FromMinutes(settings.InstallerTimeoutMinutes));

        if (processResult.IsFailure)
            return Result.Failure(processResult.Error with { Code = "NixosAnywhereInstallFailed" });

        return Result.Success();
    }

    private static List<string> BuildArguments(
        string configurationDirectoryPath,
        string ipAddress,
        string configurationName,
        string privateKeyPath,
        string certificatePath,
        string targetUser,
        bool buildOnTarget,
        bool copyHostKeys,
        bool disableHostKeyChecking,
        IReadOnlyList<string> extraArguments)
    {
        var arguments = new List<string>
        {
            "--flake",
            $"path:{configurationDirectoryPath}#{configurationName}"
        };

        if (buildOnTarget)
            arguments.Add("--build-on-remote");

        if (copyHostKeys)
            arguments.Add("--copy-host-keys");

        arguments.Add("--target-host");
        arguments.Add($"{targetUser}@{ipAddress}");

        arguments.Add("--ssh-option");
        arguments.Add($"IdentityFile={privateKeyPath}");

        arguments.Add("--ssh-option");
        arguments.Add($"CertificateFile={certificatePath}");

        arguments.Add("--ssh-option");
        arguments.Add("BatchMode=yes");

        if (disableHostKeyChecking)
        {
            arguments.Add("--ssh-option");
            arguments.Add("StrictHostKeyChecking=no");

            arguments.Add("--ssh-option");
            arguments.Add("UserKnownHostsFile=/dev/null");
        }

        if (extraArguments.Count > 0)
            arguments.AddRange(extraArguments);

        return arguments;
    }
}