using MediatR;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Setup;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;
using PhoeNix.Domain.Extensions;

namespace PhoeNix.Application.Setup.Commands;

public record AdvanceMachineSetupCommand(MachineId MachineId) : ICommand;

internal sealed class AdvanceMachineSetupCommandHandler(
    ISetupSessionRepository setupSessionRepository,
    ISetupWorkflowDecider setupWorkflowDecider,
    ISender sender)
    : ICommandHandler<AdvanceMachineSetupCommand>
{
    public async Task<Result> Handle(
        AdvanceMachineSetupCommand request,
        CancellationToken cancellationToken)
    {
        var sessionResult = await setupSessionRepository
            .GetWithEnrolledMachineAsync(request.MachineId, cancellationToken)
            .EnsureNotNull(new Error(
                "SetupSessionNotFound",
                $"No active setup session was found for machine '{request.MachineId.Value}'."));

        if (sessionResult.IsFailure)
            return sessionResult.Error;

        var session = sessionResult.Value;

        return await setupWorkflowDecider
            .Decide(session, request.MachineId)
            .Bind(decision => DispatchAsync(
                session.Id,
                request.MachineId,
                decision,
                cancellationToken));
    }

    private Task<Result> DispatchAsync(
        SetupSessionId sessionId,
        MachineId machineId,
        SetupWorkflowDecision decision,
        CancellationToken cancellationToken)
    {
        return decision.Action switch
        {
            SetupWorkflowAction.None =>
                Task.FromResult(Result.Success()),

            SetupWorkflowAction.ProbeHardware =>
                sender.Send(
                    new GetMachineHardwareInformationCommand(sessionId, machineId),
                    cancellationToken),

            SetupWorkflowAction.InstallMachine =>
                sender.Send(
                    new ApplyConfigurationToMachineCommand(machineId),
                    cancellationToken),

            _ =>
                Task.FromResult(Result.Failure(new Error(
                    "SetupWorkflowActionUnsupported",
                    $"Workflow action '{decision.Action}' is not supported.")))
        };
    }
}