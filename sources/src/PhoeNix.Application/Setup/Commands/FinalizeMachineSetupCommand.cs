using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Bootstrap;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Setup;
using PhoeNix.Application.Setup.Extensions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Commands;

public record FinalizeMachineSetupCommand(string Token, System.Net.IPAddress MachineIpAddress) : ICommand;

internal sealed class FinalizeMachineSetupCommandHandler(
    ISetupSessionRepository setupSessionRepository,
    IMachineRepository machineRepository,
    INetbootHostService netbootHostService,
    ICallbackTokenService callbackTokenService)
    : ICommandHandler<FinalizeMachineSetupCommand>
{
    public async Task<Result> Handle(
        FinalizeMachineSetupCommand request,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        var tokenContextResult = await callbackTokenService.ValidateAndDecode(request.Token, nowUtc);
        if (tokenContextResult.IsFailure)
            return tokenContextResult.Error;

        var tokenContext = tokenContextResult.Value;

        var machineResult = await machineRepository
            .GetByIdAsync(tokenContext.MachineId, cancellationToken)
            .EnsureNotNull(new Error(
                "MachineNotFound",
                $"Machine '{tokenContext.MachineId.Value}' was not found."));

        if (machineResult.IsFailure)
            return machineResult.Error;

        var sessionResult = await setupSessionRepository
            .GetByIdAsync(tokenContext.SessionId, cancellationToken)
            .EnsureNotNull(new Error(
                "SetupSessionNotFound",
                $"Setup session '{tokenContext.SessionId.Value}' was not found."));

        if (sessionResult.IsFailure)
            return sessionResult.Error;

        var session = sessionResult.Value;
        var machine = machineResult.Value;

        var target = session.Targets.FirstOrDefault(t => t.MachineId == machine.Id);
        if (target is null)
            return Result.Failure(new Error(
                "SetupTargetNotFound",
                $"Machine '{machine.Id.Value}' is not enrolled in setup session '{session.Id.Value}'."));

        if (target.Stage != SetupStage.Orchestrated)
        {
            var error = new Error(
                "SetupTargetInvalidStage",
                $"Machine '{machine.Id.Value}' must be in '{SetupStage.Orchestrated}' stage before finalization can be recorded.");

            return session.PersistFailure(
                machine.Id,
                error,
                nameof(FinalizeMachineSetupCommandHandler),
                nowUtc);
        }

        if (target.CallbackToken is null)
        {
            var error = new Error(
                "SetupCallbackTokenMissing",
                "No callback token is assigned to the setup target.");

            return session.PersistFailure(
                machine.Id,
                error,
                nameof(FinalizeMachineSetupCommandHandler),
                nowUtc);
        }

        if (target.SelectedConfigurationId is null)
        {
            var error = new Error(
                "SetupConfigurationMissing",
                "No selected configuration is assigned to the setup target.");

            return session.PersistFailure(
                machine.Id,
                error,
                nameof(FinalizeMachineSetupCommandHandler),
                nowUtc);
        }

        if (target.SelectedSystemId is null)
        {
            var error = new Error(
                "SetupSystemMissing",
                "No selected system is assigned to the setup target.");

            return session.PersistFailure(
                machine.Id,
                error,
                nameof(FinalizeMachineSetupCommandHandler),
                nowUtc);
        }

        var boundDiskPaths = target.RankedDiskAssignments
            .OrderBy(d => d.Index)
            .Select(d => d.DiskByIdPath)
            .ToList();

        var snapshotResult = machine.RecordDeploymentSnapshot(
            target.SelectedConfigurationId,
            target.SelectedSystemId,
            request.MachineIpAddress,
            nowUtc,
            boundDiskPaths);

        if (snapshotResult.IsFailure)
            return snapshotResult.Error;

        var revokeTokenResult = session.RevokeMachineCallbackToken(machine.Id, nowUtc);
        if (revokeTokenResult.IsFailure)
            return session.PersistFailure(
                machine.Id,
                revokeTokenResult.Error,
                nameof(FinalizeMachineSetupCommandHandler),
                nowUtc);

        var stageResult = session.UpdateMachineStage(
            machine.Id,
            SetupStage.Finished,
            nowUtc);

        if (stageResult.IsFailure)
            return stageResult.Error;

        var hasPendingPxeTargets = session.Targets.Any(t =>
            t.MachineId != machine.Id &&
            t.Stage is SetupStage.WaitingForPxe or SetupStage.ArtefactsAssigned);

        if (hasPendingPxeTargets)
            return Result.Success();

        var stopHostResult = await netbootHostService.StopAsync(cancellationToken);
        if (stopHostResult.IsFailure)
            return stopHostResult.Error;

        return Result.Success();
    }
}