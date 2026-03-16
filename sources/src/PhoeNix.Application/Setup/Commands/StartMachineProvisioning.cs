using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Commands;

public record StartMachineSetupCommand(
    SetupSessionId SessionId,
    MachineId MachineId) : ICommand;

internal sealed class StartMachineSetupHandler(
    ISetupSessionRepository setupSessionRepository,
    IMachineRepository machineRepository,
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

        var target = session.Targets.FirstOrDefault(t => t.MachineId == machine.Id);

        if (target is null)
        {
            var enrollResult = session.EnrollMachine(machine.Id, nowUtc);
            if (enrollResult.IsFailure)
                return enrollResult.Error;
        }

        var tokenResult = callbackTokenService.Create(
                session.Id,
                machine.Id,
                nowUtc,
                TimeSpan.FromMinutes(10))
            .Tap(_ => session.ClearCallbackToken(machine.Id))
            .Bind(token => session.AssignMachineCallbackToken(machine.Id, token));

        return tokenResult.IsFailure
            ? tokenResult.Error
            : session.UpdateMachineStage(machine.Id, SetupStage.WaitingForPxe);
    }
}