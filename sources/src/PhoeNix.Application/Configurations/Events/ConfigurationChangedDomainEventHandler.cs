using MediatR;
using PhoeNix.Application.Configurations.Commands;
using PhoeNix.Domain.Events;

namespace PhoeNix.Application.Configurations.Events;

internal sealed class ConfigurationChangedDomainEventHandler(ISender sender)
    : INotificationHandler<ConfigurationChangedDomainEvent>
{
    public Task Handle(ConfigurationChangedDomainEvent notification, CancellationToken cancellationToken)
        => sender.Send(new MarkMachinesOutDated(notification.ConfigurationId), cancellationToken);
}
