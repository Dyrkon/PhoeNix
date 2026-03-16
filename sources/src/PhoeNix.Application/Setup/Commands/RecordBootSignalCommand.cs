using System.Net;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Commands;

public record RecordBootSignalCommand(
    SetupSessionId SessionId,
    MachineId MachineId,
    string CallbackToken,
    IPAddress MachineIpAddress) : ICommand;

internal sealed class RecordBootSignalCommandHandler(
    ISetupSessionRepository setupSessionRepository,
    ICallbackTokenService callbackTokenService)
    : ICommandHandler<RecordBootSignalCommand>
{
    public async Task<Result> Handle(RecordBootSignalCommand request, CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        var tokenContextResult = await callbackTokenService.ValidateAndDecode(request.CallbackToken, nowUtc);
        if (tokenContextResult.IsFailure)
            return tokenContextResult.Error;

        var tokenContext = tokenContextResult.Value;

        if (tokenContext.SessionId != request.SessionId)
            return Result.Failure(new Error(
                "BootstrapCallbackSessionMismatch",
                "Session id from callback body does not match the callback token."));

        if (tokenContext.MachineId != request.MachineId)
            return Result.Failure(new Error(
                "BootstrapCallbackMachineMismatch",
                "Machine id from callback body does not match the callback token."));

        var sessionResult = await setupSessionRepository
            .GetByIdAsync(request.SessionId, cancellationToken)
            .EnsureNotNull(new Error(
                "SetupSessionNotFound",
                $"Setup session '{request.SessionId.Value}' was not found."));

        if (sessionResult.IsFailure)
            return sessionResult.Error;

        var session = sessionResult.Value;

        var target = session.Targets.FirstOrDefault(t => t.MachineId == request.MachineId);
        if (target is null)
            return Result.Failure(new Error(
                "SetupTargetNotFound",
                $"Machine '{request.MachineId.Value}' is not enrolled in setup session '{request.SessionId.Value}'."));

        if (target.CallbackToken is null)
            return Result.Failure(new Error(
                "SetupCallbackTokenMissing",
                "No callback token is assigned to the setup target."));

        if (!string.Equals(target.CallbackToken.Token, request.CallbackToken, StringComparison.Ordinal))
            return Result.Failure(new Error(
                "SetupCallbackTokenMismatch",
                "Setup callback token does not match the token assigned to the setup target."));

        if (!target.CallbackToken.IsValid(nowUtc))
            return Result.Failure(new Error(
                "SetupCallbackTokenInvalid",
                "The callback token is expired or revoked."));

        var recordIpResult = session.RecordMachineIpAddress(request.MachineId, request.MachineIpAddress);
        if (recordIpResult.IsFailure)
            return recordIpResult.Error;

        var revokeResult = session.RevokeMachineCallbackToken(request.MachineId, nowUtc);
        if (revokeResult.IsFailure)
            return revokeResult.Error;

        var stageResult = session.UpdateMachineStage(request.MachineId, SetupStage.Bootstrapped);
        if (stageResult.IsFailure)
            return stageResult.Error;

        return Result.Success();
    }
}