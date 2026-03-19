using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Bootstrap;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Commands;

public record FinalizeMachineSetupCommand(string Token) : ICommand;

internal sealed class FinalizeSetupSessionCommandHandler(
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

        var machineResult = await machineRepository.GetByIdAsync(tokenContext.MachineId, cancellationToken)
            .EnsureNotNull(new Error(
                "MachineNotFound",
                $"Machine '{tokenContext.MachineId.Value}' was not found."));

        if (machineResult.IsFailure)
            return machineResult.Error;

        var sessionResult = await setupSessionRepository.GetByIdAsync(tokenContext.SessionId, cancellationToken)
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

        var revokeTokenResult = session.RevokeMachineCallbackToken(machine.Id, nowUtc);
        if (revokeTokenResult.IsFailure)
            return revokeTokenResult.Error;

        var hasOtherActiveTargets = session.Targets.Any(t =>
            t.MachineId != machine.Id &&
            t.CallbackToken is not null &&
            t.CallbackToken.RevokedAtUtc is null &&
            t.CallbackToken.ExpiresAtUtc > nowUtc);

        if (hasOtherActiveTargets)
            return Result.Success();

        return await netbootHostService.StopAsync(cancellationToken);
    }
}