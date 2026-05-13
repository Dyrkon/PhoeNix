using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhoeNix.Application.Abstractions.Deployment;
using PhoeNix.Application.Abstractions.Setup;
using PhoeNix.Application.Models.Deployment;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Infrastructure.Services.Deployment;

internal sealed class DeploymentBackgroundService(
    IDeploymentJobTracker jobTracker,
    IServiceScopeFactory scopeFactory,
    ILogger<DeploymentBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in jobTracker.Reader.ReadAllAsync(stoppingToken))
        {
            jobTracker.SetStatus(job.MachineId, new DeploymentJobStatus(DeploymentJobState.Running));

            try
            {
                await ExecuteJobAsync(job, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception executing deployment for machine {MachineId}.",
                    job.MachineId.Value);
                jobTracker.SetStatus(job.MachineId, new DeploymentJobStatus(
                    DeploymentJobState.Failed,
                    "DeploymentUnhandledError",
                    ex.Message));
            }
        }
    }

    private async Task ExecuteJobAsync(DeploymentJob job, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var nixOsMachineUpdater = scope.ServiceProvider.GetRequiredService<INixOsMachineUpdater>();
        var machineRepository = scope.ServiceProvider.GetRequiredService<IMachineRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        logger.LogInformation(
            "Starting deployment for machine {MachineId}, system {SystemAttribute}.",
            job.MachineId.Value,
            job.SystemAttribute);

        var updateResult = await nixOsMachineUpdater.UpdateAsync(
            job.TargetIpAddress,
            job.TargetHostname,
            job.FlakeDirectory,
            job.SystemAttribute,
            job.SshMaterial,
            cancellationToken);

        if (updateResult.IsFailure)
        {
            logger.LogError(
                "Deployment failed for machine {MachineId}: {ErrorCode} — {ErrorMessage}",
                job.MachineId.Value,
                updateResult.Error.Code,
                updateResult.Error.Description);

            jobTracker.SetStatus(job.MachineId, new DeploymentJobStatus(
                DeploymentJobState.Failed,
                updateResult.Error.Code,
                updateResult.Error.Description));
            return;
        }

        var nowUtc = DateTime.UtcNow;

        var machine = await machineRepository.GetByIdAsync(job.MachineId, cancellationToken);
        if (machine is null)
        {
            jobTracker.SetStatus(job.MachineId, new DeploymentJobStatus(
                DeploymentJobState.Failed,
                "MachineNotFound",
                $"Machine '{job.MachineId.Value}' was not found after deployment."));
            return;
        }

        var snapshotResult = machine.RecordDeploymentSnapshot(
            job.ConfigurationId,
            job.ConfigurationTitle,
            job.SystemId,
            job.SystemName,
            job.TargetIpAddress,
            nowUtc,
            job.BoundDiskPaths);

        if (snapshotResult.IsFailure)
        {
            jobTracker.SetStatus(job.MachineId, new DeploymentJobStatus(
                DeploymentJobState.Failed,
                snapshotResult.Error.Code,
                snapshotResult.Error.Description));
            return;
        }

        var stateResult = machine.ChangeMachineState(MachineState.Updated, nowUtc);
        if (stateResult.IsFailure)
        {
            jobTracker.SetStatus(job.MachineId, new DeploymentJobStatus(
                DeploymentJobState.Failed,
                stateResult.Error.Code,
                stateResult.Error.Description));
            return;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deployment succeeded for machine {MachineId}.", job.MachineId.Value);
        jobTracker.SetStatus(job.MachineId, new DeploymentJobStatus(DeploymentJobState.Succeeded));
    }
}