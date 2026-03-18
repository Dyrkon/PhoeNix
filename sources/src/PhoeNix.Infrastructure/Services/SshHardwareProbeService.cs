using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.HardwareProbing;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Models.HardwareProbing;
using PhoeNix.Application.Options;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public sealed class SshHardwareProbeService(
    IProcessRunner processRunner,
    ISshKeyFileStore sshKeyFileStore,
    IOptions<HardwareProbeOptions> hardwareProbeOptions)
    : IHardwareProbeService
{
    private readonly HardwareProbeOptions _options = hardwareProbeOptions.Value;

    public Task<Result<HardwareProbeResult>> ProbeAsync(
        SetupSession session,
        MachineId machineId,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        if (session.SshCredential is null)
            return Task.FromResult(Result.Failure<HardwareProbeResult>(new Error(
                "HardwareProbeSshCredentialMissing",
                "The setup session does not have an SSH credential assigned.")));

        if (!session.SshCredential.IsValid(nowUtc))
            return Task.FromResult(Result.Failure<HardwareProbeResult>(new Error(
                "HardwareProbeSshCredentialInvalid",
                "The setup session SSH credential is expired or revoked.")));

        var target = session.Targets.FirstOrDefault(t => t.MachineId == machineId);
        if (target is null)
            return Task.FromResult(Result.Failure<HardwareProbeResult>(new Error(
                "HardwareProbeTargetNotFound",
                $"Machine '{machineId.Value}' is not enrolled in setup session '{session.Id.Value}'.")));

        if (target.IpAddress is null)
            return Task.FromResult(Result.Failure<HardwareProbeResult>(new Error(
                "HardwareProbeTargetIpMissing",
                $"Machine '{machineId.Value}' does not have a recorded IP address.")));

        var keyPathsResult = sshKeyFileStore.GetSessionKeyPaths(session.Id);
        if (keyPathsResult.IsFailure)
            return Task.FromResult(Result.Failure<HardwareProbeResult>(keyPathsResult.Error));

        var keyPaths = keyPathsResult.Value;

        var permissionResult = sshKeyFileStore.EnsurePrivateKeyPermissions(keyPaths.PrivateKeyPath);
        if (permissionResult.IsFailure)
            return Task.FromResult(Result.Failure<HardwareProbeResult>(permissionResult.Error));

        var arguments = BuildSshArguments(
            target.IpAddress.ToString(),
            keyPaths.PrivateKeyPath,
            keyPaths.CertificatePath);

        var processResult = processRunner.RunProcess(
            _options.SshExecutable,
            arguments,
            cancellationToken,
            timeOut: TimeSpan.FromSeconds(_options.ProbeTimeoutSeconds));

        if (processResult.IsFailure)
            return Task.FromResult(Result.Failure<HardwareProbeResult>(new Error(
                "HardwareProbeExecutionFailed",
                processResult.Error.Description)));

        var stdout = processResult.Value.StandardOutput?.Trim();
        if (string.IsNullOrWhiteSpace(stdout))
            return Task.FromResult(Result.Failure<HardwareProbeResult>(new Error(
                "HardwareProbeEmptyOutput",
                "The hardware probe completed but did not return any report content.")));

        return Task.FromResult(Result.Success(new HardwareProbeResult(
            stdout,
            DateTime.UtcNow)));
    }

    private List<string> BuildSshArguments(
        string host,
        string privateKeyPath,
        string certificatePath)
    {
        var arguments = new List<string>
        {
            "-o", "BatchMode=yes",
            "-o", $"ConnectTimeout={_options.ConnectTimeoutSeconds}",
            "-i", privateKeyPath,
            "-o", $"CertificateFile={certificatePath}"
        };

        if (_options.DisableHostKeyChecking)
        {
            arguments.Add("-o");
            arguments.Add("StrictHostKeyChecking=no");
            arguments.Add("-o");
            arguments.Add("UserKnownHostsFile=/dev/null");
        }

        arguments.Add($"{_options.BootstrapUser}@{host}");
        arguments.Add(_options.ProbeCommand);

        return arguments;
    }
}