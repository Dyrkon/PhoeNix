using System.Net;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Setup;
using PhoeNix.Application.Setup.Extensions;
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
    ICallbackTokenService callbackTokenService,
    IMachineRepository machineRepository)
    : ICommandHandler<RecordBootSignalCommand>
{
    public async Task<Result> Handle(
        RecordBootSignalCommand request,
        CancellationToken cancellationToken)
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

        if (target.Stage != SetupStage.ArtefactsAssigned)
        {
            var error = new Error(
                "SetupTargetInvalidStage",
                $"Machine '{request.MachineId.Value}' must be in '{SetupStage.ArtefactsAssigned}' stage before bootstrap callback can be recorded.");

            return session.PersistFailure(
                request.MachineId,
                error,
                nameof(RecordBootSignalCommandHandler),
                nowUtc);
        }

        if (target.CallbackToken is null)
        {
            var error = new Error(
                "SetupCallbackTokenMissing",
                "No callback token is assigned to the setup target.");

            return session.PersistFailure(
                request.MachineId,
                error,
                nameof(RecordBootSignalCommandHandler),
                nowUtc);
        }

        if (!string.Equals(target.CallbackToken.Token, request.CallbackToken, StringComparison.Ordinal))
        {
            var error = new Error(
                "SetupCallbackTokenMismatch",
                "Setup callback token does not match the token assigned to the setup target.");

            return session.PersistFailure(
                request.MachineId,
                error,
                nameof(RecordBootSignalCommandHandler),
                nowUtc);
        }

        if (!target.CallbackToken.IsValid(nowUtc))
        {
            var error = new Error(
                "SetupCallbackTokenInvalid",
                "The callback token is expired or revoked.");

            return session.PersistFailure(
                request.MachineId,
                error,
                nameof(RecordBootSignalCommandHandler),
                nowUtc);
        }

        var recordIpResult = session.RecordMachineIpAddress(request.MachineId, request.MachineIpAddress);
        if (recordIpResult.IsFailure)
            return session.PersistFailure(
                request.MachineId,
                recordIpResult.Error,
                nameof(RecordBootSignalCommandHandler),
                nowUtc);

        var stageResult = session.UpdateMachineStage(
            request.MachineId,
            SetupStage.Bootstrapped,
            nowUtc);

        if (stageResult.IsFailure)
            return stageResult.Error;

        var machineResult = await machineRepository
            .GetByIdAsync(request.MachineId, cancellationToken)
            .EnsureNotNull(new Error(
                "MachineNotFound",
                $"Machine '{request.MachineId.Value}' was not found."));

        if (machineResult.IsFailure)
            return machineResult.Error;

        return machineResult.Value.ChangeMachineState(MachineState.Registered, nowUtc);
    }
}