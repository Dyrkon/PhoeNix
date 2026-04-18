using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PhoeNix.Application.Setup.Commands;
using PhoeNix.Application.Setup.Events;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Events;

namespace PhoeNix.Application.UnitTests.EventHandlers;

public class SetupTargetStageChangedDomainEventHandlerTests
{
    private readonly ISender _sender = Substitute.For<ISender>();
    private readonly NullLogger<SetupTargetStageChangedDomainEventHandler> _logger = new();

    [Theory]
    [InlineData(SetupStage.Bootstrapped)]
    [InlineData(SetupStage.Probed)]
    public async Task Handle_Should_Send_AdvanceMachineSetupCommand_When_Stage_Is_Bootstrapped_Or_Probed(SetupStage stage)
    {
        var handler = new SetupTargetStageChangedDomainEventHandler(_sender, _logger);
        var machineId = new MachineId(Guid.NewGuid());
        var sessionId = new SetupSessionId(Guid.NewGuid());
        var domainEvent = new SetupTargetStageChangedDomainEvent(
            sessionId, machineId, SetupStage.WaitingForPxe, stage);

        await handler.Handle(domainEvent, CancellationToken.None);

        await _sender.Received(1).Send(
            Arg.Is<AdvanceMachineSetupCommand>(cmd => cmd.MachineId == machineId),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(SetupStage.Created)]
    [InlineData(SetupStage.WaitingForPxe)]
    [InlineData(SetupStage.Finished)]
    [InlineData(SetupStage.Cancelled)]
    [InlineData(SetupStage.Failed)]
    public async Task Handle_Should_Not_Send_Command_For_Other_Stages(SetupStage stage)
    {
        var handler = new SetupTargetStageChangedDomainEventHandler(_sender, _logger);
        var machineId = new MachineId(Guid.NewGuid());
        var sessionId = new SetupSessionId(Guid.NewGuid());
        var domainEvent = new SetupTargetStageChangedDomainEvent(
            sessionId, machineId, SetupStage.Created, stage);

        await handler.Handle(domainEvent, CancellationToken.None);

        await _sender.DidNotReceive().Send(Arg.Any<IRequest>(), Arg.Any<CancellationToken>());
    }
}
