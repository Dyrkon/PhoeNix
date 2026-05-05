using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Bootstrap;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Commands;

public record CancelSetupSessionCommand(SetupSessionId SessionId) : ICommand;

internal sealed class CancelSetupSessionCommandHandler(
    ISetupSessionRepository setupSessionRepository,
    INetbootHostService netbootHostService,
    ICurrentUserAccessor currentUserAccessor)
    : ICommandHandler<CancelSetupSessionCommand>
{
    public async Task<Result> Handle(
        CancelSetupSessionCommand request,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return userIdResult.Error;

        var sessionResult = await setupSessionRepository
            .GetByIdAsync(request.SessionId, cancellationToken)
            .EnsureNotNull(SetupSessionErrors.NotFound(request.SessionId))
            .Ensure(s => s.OwnerId == userIdResult.Value, SetupSessionErrors.NotFound(request.SessionId));

        if (sessionResult.IsFailure)
            return sessionResult;

        var session = sessionResult.Value;

        var revokeSshResult = session.RevokeSshCredential(nowUtc);
        if (revokeSshResult.IsFailure &&
            revokeSshResult.Error.Code != "SetupSessionSshCredentialMissing")
            return revokeSshResult;

        foreach (var target in session.Targets)
        {
            if (target.CallbackToken is not null)
            {
                var revokeTokenResult = session.RevokeMachineCallbackToken(target.MachineId, nowUtc);
                if (revokeTokenResult.IsFailure)
                    return revokeTokenResult;
            }

            if (target.Stage is SetupStage.Finished or SetupStage.Cancelled)
                continue;

            var stageResult = session.UpdateMachineStage(target.MachineId, SetupStage.Cancelled, nowUtc);
            if (stageResult.IsFailure)
                return stageResult;
        }

        var stopHostResult = await netbootHostService.StopAsync(cancellationToken);
        if (stopHostResult.IsFailure)
            return stopHostResult;

        return Result.Success();
    }
}