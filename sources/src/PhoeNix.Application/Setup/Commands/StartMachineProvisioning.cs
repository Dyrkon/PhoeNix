using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
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

        var session = sessionResult.Value;
        var machine = machineResult.Value;

        var configurationResult = await configurationRepository
            .GetByIdAsync(request.ConfigurationId, cancellationToken)
            .EnsureNotNull(new Error(
                "ConfigurationNotFound",
                $"Configuration '{request.ConfigurationId.ToString()}' was not found."));

        if (configurationResult.IsFailure)
            return configurationResult.Error;

        var configuration = configurationResult.Value;

        var systemExists = configuration.SystemSpecifications
            .Any(s => s.Id == request.SystemId);

        if (!systemExists)
            return Result.Failure(new Error(
                "SystemNotInConfiguration",
                $"System '{request.SystemId.Value}' is not part of configuration '{configuration.Id.Value}'."));

        var target = session.Targets.FirstOrDefault(t => t.MachineId == machine.Id);

        if (target is null)
        {
            var enrollResult = session.EnrollMachine(machine.Id, request.SystemId, request.ConfigurationId, nowUtc);
            if (enrollResult.IsFailure)
                return enrollResult.Error;

            target = session.Targets.First(t => t.MachineId == machine.Id);
        }
        else
        {
            if (target.SelectedSystemId != request.SystemId)
                return Result.Failure(new Error(
                    "SystemMismatch",
                    "System cannot be changed for already enrolled machine."));
        }

        session.ClearCallbackToken(machine.Id);
        session.ClearRankedDisks(machine.Id);

        var tokenResult = callbackTokenService.Create(
                session.Id,
                machine.Id,
                nowUtc,
                TimeSpan.FromMinutes(10))
            .Bind(token => session.AssignMachineCallbackToken(machine.Id, token));

        if (tokenResult.IsFailure)
            return tokenResult.Error;

        return session.UpdateMachineStage(machine.Id, SetupStage.WaitingForPxe);
    }
}