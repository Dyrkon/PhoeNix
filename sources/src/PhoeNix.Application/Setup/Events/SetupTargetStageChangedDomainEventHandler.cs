using MediatR;
using PhoeNix.Application.Setup.Commands;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Events;

namespace PhoeNix.Application.Setup.Events;

internal sealed class SetupTargetStageChangedDomainEventHandler(ISender sender)
    : INotificationHandler<SetupTargetStageChangedDomainEvent>
{
    public async Task Handle(
        SetupTargetStageChangedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        if (notification.CurrentStage is not (SetupStage.Bootstrapped or SetupStage.Probed))
            return;

        await sender.Send(
            new AdvanceMachineSetupCommand(notification.MachineId),
            cancellationToken);
    }
}