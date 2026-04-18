using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.HardwareProbing;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Models.HardwareProbing;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.HardwareManagement;

public sealed class SshHardwareProbeService(
    IProcessRunner processRunner,
    ISshKeyFileStore sshKeyFileStore,
    IAppSettingsRepository settingsRepository)
    : IHardwareProbeService
{
    public async Task<Result<HardwareProbeResult>> ProbeAsync(
        SetupSession session,
        MachineId machineId,
        CancellationToken cancellationToken)
    {
        var settings = await settingsRepository.GetAsync(cancellationToken);
        if (settings is null)
            return Result.Failure<HardwareProbeResult>(new Error(
                "AppSettings.NotFound",
                "Application settings have not been initialized."));

        var nowUtc = DateTime.UtcNow;

        if (session.SshCredential is null)
            return Result.Failure<HardwareProbeResult>(new Error(
                "HardwareProbeSshCredentialMissing",
                "The setup session does not have an SSH credential assigned."));

        if (!session.SshCredential.IsValid(nowUtc))
            return Result.Failure<HardwareProbeResult>(new Error(
                "HardwareProbeSshCredentialInvalid",
                "The setup session SSH credential is expired or revoked."));

        var target = session.Targets.FirstOrDefault(t => t.MachineId == machineId);
        if (target is null)
            return Result.Failure<HardwareProbeResult>(new Error(
                "HardwareProbeTargetNotFound",
                $"Machine '{machineId.Value}' is not enrolled in setup session '{session.Id.Value}'."));

        if (target.IpAddress is null)
            return Result.Failure<HardwareProbeResult>(new Error(
                "HardwareProbeTargetIpMissing",
                $"Machine '{machineId.Value}' does not have a recorded IP address."));

        var keyPathsResult = sshKeyFileStore.GetSessionKeyPaths(session.Id);
        if (keyPathsResult.IsFailure)
            return Result.Failure<HardwareProbeResult>(keyPathsResult.Error);

        var keyPaths = keyPathsResult.Value;

        var permissionResult = sshKeyFileStore.EnsurePrivateKeyPermissions(keyPaths.PrivateKeyPath);
        if (permissionResult.IsFailure)
            return Result.Failure<HardwareProbeResult>(permissionResult.Error);

        var arguments = BuildSshArguments(
            target.IpAddress.ToString(),
            keyPaths.PrivateKeyPath,
            keyPaths.CertificatePath,
            settings.HardwareProbeConnectTimeoutSeconds,
            settings.HardwareProbeDisableHostKeyChecking,
            settings.HardwareProbeBootstrapUser,
            settings.HardwareProbeProbeCommand);

        var processResult = processRunner.RunProcess(
            settings.HardwareProbeSshExecutable,
            arguments,
            cancellationToken,
            timeOut: TimeSpan.FromSeconds(settings.HardwareProbeProbeTimeoutSeconds));

        if (processResult.IsFailure)
            return Result.Failure<HardwareProbeResult>(new Error(
                "HardwareProbeExecutionFailed",
                processResult.Error.Description));

        var stdout = processResult.Value.StandardOutput?.Trim();
        if (string.IsNullOrWhiteSpace(stdout))
            return Result.Failure<HardwareProbeResult>(new Error(
                "HardwareProbeEmptyOutput",
                "The hardware probe completed but did not return any report content."));

        return Result.Success(new HardwareProbeResult(stdout, DateTime.UtcNow));
    }

    private static List<string> BuildSshArguments(
        string host,
        string privateKeyPath,
        string certificatePath,
        int connectTimeoutSeconds,
        bool disableHostKeyChecking,
        string bootstrapUser,
        string probeCommand)
    {
        var arguments = new List<string>
        {
            "-o", "BatchMode=yes",
            "-o", $"ConnectTimeout={connectTimeoutSeconds}",
            "-i", privateKeyPath,
            "-o", $"CertificateFile={certificatePath}"
        };

        if (disableHostKeyChecking)
        {
            arguments.Add("-o");
            arguments.Add("StrictHostKeyChecking=no");
            arguments.Add("-o");
            arguments.Add("UserKnownHostsFile=/dev/null");
        }

        arguments.Add($"{bootstrapUser}@{host}");
        arguments.Add(probeCommand);

        return arguments;
    }
}
