using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Provosioning.Commands;

public record RecordBootSignalCommand(
    ProvisioningSessionId SessionId,
    MachineId MachineId,
    string CallbackToken) : ICommand;

internal sealed class RecordBootSignalCommandHandler(
    IProvisioningSessionRepository provisioningSessionRepository,
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

        var sessionResult = await provisioningSessionRepository
            .GetByIdAsync(request.SessionId, cancellationToken)
            .EnsureNotNull(new Error(
                "ProvisioningSessionNotFound",
                $"Provisioning session '{request.SessionId.Value}' was not found."));

        if (sessionResult.IsFailure)
            return sessionResult.Error;

        var session = sessionResult.Value;

        var target = session.Targets.FirstOrDefault(t => t.MachineId == request.MachineId);
        if (target is null)
            return Result.Failure(new Error(
                "ProvisioningTargetNotFound",
                $"Machine '{request.MachineId.Value}' is not enrolled in provisioning session '{request.SessionId.Value}'."));

        if (target.CallbackToken is null)
            return Result.Failure(new Error(
                "ProvisioningCallbackTokenMissing",
                "No callback token is assigned to the provisioning target."));

        if (!string.Equals(target.CallbackToken.Token, request.CallbackToken, StringComparison.Ordinal))
            return Result.Failure(new Error(
                "ProvisioningCallbackTokenMismatch",
                "Provided callback token does not match the token assigned to the provisioning target."));

        if (!target.CallbackToken.IsValid(nowUtc))
            return Result.Failure(new Error(
                "ProvisioningCallbackTokenInvalid",
                "The callback token is expired or revoked."));

        var revokeResult = session.RevokeMachineCallbackToken(request.MachineId, nowUtc);
        if (revokeResult.IsFailure)
            return revokeResult.Error;

        // Todo this is overloaded a lot: ProvisioningStage.SecretsGenerated
        var stageResult = session.UpdateMachineStage(request.MachineId, ProvisioningStage.SecretsGenerated);
        if (stageResult.IsFailure)
            return stageResult.Error;

        Console.WriteLine($"SUCCESS");
        return Result.Success();
    }
}