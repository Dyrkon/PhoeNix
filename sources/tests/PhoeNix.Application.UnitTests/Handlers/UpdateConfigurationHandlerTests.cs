using FluentAssertions;
using NSubstitute;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Configurations.Commands;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.UnitTests.Handlers;

public class UpdateConfigurationHandlerTests
{
    private static readonly UserId OwnerId = new(Guid.NewGuid());

    private readonly IConfigurationRepository _configurationRepository =
        Substitute.For<IConfigurationRepository>();

    private readonly IModuleTemplateRepository _moduleTemplateRepository =
        Substitute.For<IModuleTemplateRepository>();

    private readonly ICurrentUserAccessor _currentUserAccessor =
        Substitute.For<ICurrentUserAccessor>();

    public UpdateConfigurationHandlerTests()
    {
        _currentUserAccessor.GetUserId().Returns(Result.Success(OwnerId));
    }

    [Fact]
    public async Task Handle_Should_Update_And_Return_Dto()
    {
        var configId = new ConfigurationId(Guid.NewGuid());
        var config = Configuration.Create(configId, OwnerId, "Old Title", "Old Desc").Value;
        _configurationRepository.GetByIdAsync(configId, Arg.Any<CancellationToken>())
            .Returns(config);
        _moduleTemplateRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<ModuleTemplateId>>(), Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(new List<ModuleTemplate>());

        var handler = new UpdateConfigurationHandler(_configurationRepository, _moduleTemplateRepository, _currentUserAccessor);
        var command = new UpdateConfigurationCommand(configId, "New Title", "New Desc");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("New Title");
        result.Value.Description.Should().Be("New Desc");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Configuration_Not_Found()
    {
        var configId = new ConfigurationId(Guid.NewGuid());
        _configurationRepository.GetByIdAsync(configId, Arg.Any<CancellationToken>())
            .Returns((Configuration?)null);

        var handler = new UpdateConfigurationHandler(_configurationRepository, _moduleTemplateRepository, _currentUserAccessor);
        var command = new UpdateConfigurationCommand(configId, "New Title", "New Desc");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Configurations.NotFound");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_New_Title_Empty()
    {
        var configId = new ConfigurationId(Guid.NewGuid());
        var config = Configuration.Create(configId, OwnerId, "Old Title", "Old Desc").Value;
        _configurationRepository.GetByIdAsync(configId, Arg.Any<CancellationToken>())
            .Returns(config);

        var handler = new UpdateConfigurationHandler(_configurationRepository, _moduleTemplateRepository, _currentUserAccessor);
        var command = new UpdateConfigurationCommand(configId, "", "New Desc");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Configuration title can't be blank.");
    }
}
