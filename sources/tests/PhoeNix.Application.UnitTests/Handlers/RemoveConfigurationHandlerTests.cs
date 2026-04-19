using FluentAssertions;
using NSubstitute;
using PhoeNix.Application.Configurations.Commands;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.UnitTests.Handlers;

public class RemoveConfigurationHandlerTests
{
    private readonly IConfigurationRepository _configurationRepository =
        Substitute.For<IConfigurationRepository>();

    [Fact]
    public async Task Handle_Should_Remove_Configuration()
    {
        var configId = new ConfigurationId(Guid.NewGuid());
        var config = Configuration.Create(configId, "Title", "Desc").Value;
        _configurationRepository.GetByIdAsync(configId, Arg.Any<CancellationToken>())
            .Returns(config);
        _configurationRepository.RemoveByIdAsync(configId, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var handler = new RemoveConfigurationCommandHandler(_configurationRepository);
        var command = new RemoveConfigurationCommand(configId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _configurationRepository.Received(1).RemoveByIdAsync(configId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Configuration_Not_Found()
    {
        var configId = new ConfigurationId(Guid.NewGuid());
        _configurationRepository.GetByIdAsync(configId, Arg.Any<CancellationToken>())
            .Returns((Configuration?)null);

        var handler = new RemoveConfigurationCommandHandler(_configurationRepository);
        var command = new RemoveConfigurationCommand(configId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        await _configurationRepository.DidNotReceive().RemoveByIdAsync(Arg.Any<ConfigurationId>(), Arg.Any<CancellationToken>());
    }
}
