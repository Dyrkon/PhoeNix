using FluentAssertions;
using NSubstitute;
using PhoeNix.Application.Configurations.Commands;
using PhoeNix.Application.Repositories;

namespace PhoeNix.Application.UnitTests.Handlers;

public class CreateConfigurationHandlerTests
{
    private readonly IConfigurationRepository _configurationRepository =
        Substitute.For<IConfigurationRepository>();

    [Fact]
    public async Task Handle_Should_Create_Configuration_And_Return_Dto()
    {
        var handler = new CreateConfigurationHandler(_configurationRepository);
        var command = new CreateConfigurationCommand("My Config", "A description");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("My Config");
        result.Value.Description.Should().Be("A description");
        _configurationRepository.Received(1).Add(Arg.Any<PhoeNix.Domain.Entities.Configurations.Configuration>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Title_Empty()
    {
        var handler = new CreateConfigurationHandler(_configurationRepository);
        var command = new CreateConfigurationCommand("", "A description");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Configuration title can't be blank.");
        _configurationRepository.DidNotReceive().Add(Arg.Any<PhoeNix.Domain.Entities.Configurations.Configuration>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Description_Empty()
    {
        var handler = new CreateConfigurationHandler(_configurationRepository);
        var command = new CreateConfigurationCommand("My Config", "");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Configuration description can't be blank.");
    }
}
