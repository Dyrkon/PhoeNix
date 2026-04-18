using MediatR;
using NSubstitute;
using PhoeNix.Application.Configurations.Commands;
using PhoeNix.Application.Configurations.Events;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Events;

namespace PhoeNix.Application.UnitTests.EventHandlers;

public class ConfigurationChangedDomainEventHandlerTests
{
    [Fact]
    public async Task Handle_Should_Send_MarkMachinesOutDated_Command()
    {
        var sender = Substitute.For<ISender>();
        var handler = new ConfigurationChangedDomainEventHandler(sender);
        var configId = new ConfigurationId(Guid.NewGuid());
        var domainEvent = new ConfigurationChangedDomainEvent(configId);

        await handler.Handle(domainEvent, CancellationToken.None);

        await sender.Received(1).Send(
            Arg.Is<MarkMachinesOutDated>(cmd => cmd.ConfigurationId == configId),
            Arg.Any<CancellationToken>());
    }
}
