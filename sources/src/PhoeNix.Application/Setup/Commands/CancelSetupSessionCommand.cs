using PhoeNix.Application.Abstractions.Bootstrap;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Commands;

public record CancelSetupSessionCommand(SetupSessionId SessionId) : ICommand;

internal sealed class CancelSetupSessionCommandHandler(
    ISetupSessionRepository setupSessionRepository,
    INetbootHostService netbootHostService)
    : ICommandHandler<CancelSetupSessionCommand>
{
    public async Task<Result> Handle(
        CancelSetupSessionCommand request,
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

        var session = sessionResult.Value;

        var revokeSshResult = session.RevokeSshCredential(nowUtc);
        if (revokeSshResult.IsFailure &&
            revokeSshResult.Error.Code != "SetupSessionSshCredentialMissing")
            return revokeSshResult.Error;

        foreach (var target in session.Targets)
        {
            session.RevokeMachineCallbackToken(target.MachineId, nowUtc);
            var stageResult = session.UpdateMachineStage(target.MachineId, SetupStage.Cancelled);
            if (stageResult.IsFailure)
                return stageResult.Error;
        }

        await netbootHostService.StopAsync(cancellationToken);

        return Result.Success();
    }
}