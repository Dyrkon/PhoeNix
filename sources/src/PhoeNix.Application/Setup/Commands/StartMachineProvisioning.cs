using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Commands;

public record StartMachineSetupCommand(
    SetupSessionId SessionId,
    MachineId MachineId,
    ConfigurationId ConfigurationId,
    SystemId SystemId) : ICommand;

internal sealed class StartMachineSetupHandler(
    ISetupSessionRepository setupSessionRepository,
    IMachineRepository machineRepository,
    IConfigurationRepository configurationRepository,
    ICallbackTokenService callbackTokenService)
    : ICommandHandler<StartMachineSetupCommand>
{
    public async Task<Result> Handle(
        StartMachineSetupCommand request,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        var sessionResult = await setupSessionRepository
            .GetByIdAsync(request.SessionId, cancellationToken)
            .EnsureNotNull(new Error(
                "SetupSessionNotFound",
                $"Setup session '{request.SessionId.Value}' was not found."));

        if (sessionResult.IsFailure)
            return sessionResult.Error;

        var machineResult = await machineRepository
            .GetByIdAsync(request.MachineId, cancellationToken)
            .EnsureNotNull(new Error(
                "MachineNotFound",
                $"Machine '{request.MachineId.Value}' was not found."));

        if (machineResult.IsFailure)
            return machineResult.Error;

        var configurationResult = await configurationRepository
            .GetByIdAsync(request.ConfigurationId, cancellationToken)
            .EnsureNotNull(new Error(
                "ConfigurationNotFound",
                $"Configuration '{request.ConfigurationId.Value}' was not found."));

        if (configurationResult.IsFailure)
            return configurationResult.Error;

        var session = sessionResult.Value;
        var machine = machineResult.Value;
        var configuration = configurationResult.Value;

        var systemExists = configuration.SystemSpecifications.Any(s => s.Id == request.SystemId);
        if (!systemExists)
            return Result.Failure(new Error(
                "SystemNotInConfiguration",
                $"System '{request.SystemId.Value}' is not part of configuration '{configuration.Id.Value}'."));

        var target = session.Targets.FirstOrDefault(t => t.MachineId == machine.Id);

        if (target is not null && IsActive(target.Stage))
            return Result.Failure(new Error(
                "SetupAlreadyInProgress",
                $"Machine '{machine.Id.Value}' is already in active setup stage '{target.Stage}'."));

        if (target is null)
        {
            var enrollResult = session.EnrollMachine(
                machine.Id,
                request.SystemId,
                request.ConfigurationId,
                nowUtc);

            if (enrollResult.IsFailure)
                return enrollResult.Error;

            target = session.Targets.First(t => t.MachineId == machine.Id);
        }
        else
        {
            if (target.SelectedSystemId != request.SystemId ||
                target.SelectedConfigurationId != request.ConfigurationId)
                return Result.Failure(new Error(
                    "SetupTargetSelectionMismatch",
                    "Configuration or system cannot be changed for an existing setup target."));
        }

        if (target.CallbackToken is not null)
        {
            var clearTokenResult = session.ClearCallbackToken(machine.Id);
            if (clearTokenResult.IsFailure)
                return clearTokenResult.Error;
        }

        if (target.RankedDiskAssignments.Any())
        {
            var clearRankedDisksResult = session.ClearRankedDisks(machine.Id);
            if (clearRankedDisksResult.IsFailure)
                return clearRankedDisksResult.Error;
        }

        var assignTokenResult = callbackTokenService
            .Create(session.Id, machine.Id, nowUtc, TimeSpan.FromHours(2))
            .Bind(token => session.AssignMachineCallbackToken(machine.Id, token));

        if (assignTokenResult.IsFailure)
            return assignTokenResult.Error;

        return session.UpdateMachineStage(machine.Id, SetupStage.WaitingForPxe, nowUtc)
            .Tap(() =>
                machineResult.Value.ChangeMachineState(MachineState.Registered, nowUtc));
    }

    private static bool IsActive(SetupStage stage)
    {
        return stage is SetupStage.WaitingForPxe
            or SetupStage.ArtefactsAssigned
            or SetupStage.Bootstrapped
            or SetupStage.Probed
            or SetupStage.Orchestrated;
    }
}