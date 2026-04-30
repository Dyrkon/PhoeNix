using MediatR;
using PhoeNix.Application.Abstractions.Bootstrap;
using PhoeNix.Domain.Events;

namespace PhoeNix.Application.Setup.Events;

internal sealed class SetupSessionBootstrapRequestedDomainEventHandler(
    IBootstrapSessionQueue bootstrapSessionQueue)
    : INotificationHandler<SetupSessionBootstrapRequestedDomainEvent>
{
    public Task Handle(
        SetupSessionBootstrapRequestedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        bootstrapSessionQueue.Enqueue(notification.SessionId);
        return Task.CompletedTask;
    }
}
