using System.Net;
using Microsoft.Extensions.Logging;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Abstractions.Setup;
using PhoeNix.Application.Models.Processes;
using PhoeNix.Application.Models.SshIdentity;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.UtilityWrappers;

internal sealed class NixOsMachineUpdater(
    IProcessRunner processRunner,
    ILogger<NixOsMachineUpdater> logger,
    IAppSettingsRepository settingsRepository)
    : INixOsMachineUpdater
{
    public async Task<Result<ProcessResult>> UpdateAsync(
        IPAddress targetIpAddress,
        string flakeDirectory,
        string systemAttribute,
        DeploySshAccessMaterial deployIdentity,
        CancellationToken cancellationToken)
    {
        var settings = await settingsRepository.GetAsync(cancellationToken);
        if (settings is null)
            return Result.Failure<ProcessResult>(new Error(
                "AppSettings.NotFound",
                "Application settings have not been initialized."));

        var targetHost = $"{deployIdentity.DeployUser}@{targetIpAddress}";
        var arguments = new List<string>
        {
            "switch",
            "--flake",
            $"{flakeDirectory}#{systemAttribute}",
            "--target-host",
            targetHost
        };

        if (!string.IsNullOrWhiteSpace(settings.UpdaterBuildHost))
        {
            arguments.Add("--build-host");
            arguments.Add(settings.UpdaterBuildHost.Trim());
        }

        if (settings.UpdaterUseRemoteSudo)
            arguments.Add("--sudo");

        if (settings.UpdaterFast)
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
            settings.UpdaterBuildHost,
            systemAttribute);

        var nixosRebuildPath = Environment.GetEnvironmentVariable("PHOENIX_NIXOS_REBUILD_PATH") ?? "nixos-rebuild";
        var result = processRunner.RunProcess(
            nixosRebuildPath,
            arguments,
            cancellationToken,
            environmentVariables,
            perLineAction: line =>
            {
                if (!string.IsNullOrWhiteSpace(line))
                    logger.LogInformation("nixos-rebuild: {Line}", line);
            });

        if (result.IsFailure)
            return Result.Failure<ProcessResult>(result.Error);

        return result;
    }
}
