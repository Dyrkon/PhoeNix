using MediatR;
using Microsoft.Extensions.Logging;
using PhoeNix.Application.Setup.Commands;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Events;

namespace PhoeNix.Application.Setup.Events;

internal sealed class SetupTargetStageChangedDomainEventHandler(
    ISender sender,
    ILogger<SetupTargetStageChangedDomainEventHandler> logger)
    : INotificationHandler<SetupTargetStageChangedDomainEvent>
{
    public async Task Handle(
        SetupTargetStageChangedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Setup target stage changed for machine {MachineId}: {PreviousStage} -> {CurrentStage}",
            notification.MachineId.Value,
            notification.PreviousStage,
            notification.CurrentStage);

        if (notification.CurrentStage is not (SetupStage.Bootstrapped or SetupStage.Probed))
            return;

        logger.LogInformation(
            "Dispatching workflow advance for machine {MachineId}",
            notification.MachineId.Value);

        await sender.Send(
            new AdvanceMachineSetupCommand(notification.MachineId),
            cancellationToken);
    }
}