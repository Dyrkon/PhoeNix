using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Abstractions.Setup;
using PhoeNix.Application.Models.Processes;
using PhoeNix.Application.Models.SshIdentity;
using PhoeNix.Application.Options;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

internal sealed class NixOsMachineUpdater(
    IProcessRunner processRunner,
    ILogger<NixOsMachineUpdater> logger,
    IOptions<NixOsUpdaterOptions> updaterOptions)
    : INixOsMachineUpdater
{
    public Task<Result<ProcessResult>> UpdateAsync(
        IPAddress targetIpAddress,
        string flakeDirectory,
        string systemAttribute,
        DeploySshAccessMaterial deployIdentity,
        CancellationToken cancellationToken)
    {
        var targetHost = $"{deployIdentity.DeployUser}@{targetIpAddress}";
        var arguments = new List<string>
        {
            "switch",
            "--flake",
            $"{flakeDirectory}#{systemAttribute}",
            "--target-host",
            targetHost
        };

        if (!string.IsNullOrWhiteSpace(updaterOptions.Value.BuildHost))
        {
            arguments.Add("--build-host");
            arguments.Add(updaterOptions.Value.BuildHost.Trim());
        }

        if (updaterOptions.Value.UseRemoteSudo)
            arguments.Add("--sudo");

        if (updaterOptions.Value.Fast)
            arguments.Add("--no-reexec");

        var sshOptions =
            $"-i {deployIdentity.PrivateKeyPath} " +
            $"-o CertificateFile={deployIdentity.CertificatePath} " +
            "-o IdentitiesOnly=yes " +
            "-o StrictHostKeyChecking=accept-new";

        var environmentVariables = new Dictionary<string, string>
        {
            ["NIX_SSHOPTS"] = sshOptions
        };

        logger.LogInformation(
            "Updating machine via nixos-rebuild. TargetHost={TargetHost}, BuildHost={BuildHost}, SystemAttribute={SystemAttribute}",
            targetHost,
            updaterOptions.Value.BuildHost,
            systemAttribute);

        var result = processRunner.RunProcess(
            "nixos-rebuild",
            arguments,
            cancellationToken,
            environmentVariables,
            perLineAction: line =>
            {
                if (!string.IsNullOrWhiteSpace(line))
                    logger.LogInformation("nixos-rebuild: {Line}", line);
            });

        if (result.IsFailure)
            return Task.FromResult(Result.Failure<ProcessResult>(result.Error));

        return Task.FromResult(result);
    }
}