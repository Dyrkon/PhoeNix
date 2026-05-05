using FluentAssertions;
using NSubstitute;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Configurations.Queries;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.UnitTests.Handlers;

public class GetConfigurationByIdHandlerTests
{
    private static readonly UserId OwnerId = new(Guid.NewGuid());

    private readonly IConfigurationRepository _configurationRepository =
        Substitute.For<IConfigurationRepository>();

    private readonly IModuleTemplateRepository _moduleTemplateRepository =
        Substitute.For<IModuleTemplateRepository>();

    private readonly ICurrentUserAccessor _currentUserAccessor =
        Substitute.For<ICurrentUserAccessor>();

    public GetConfigurationByIdHandlerTests()
    {
        _currentUserAccessor.GetUserId().Returns(Result.Success(OwnerId));
    }

    [Fact]
    public async Task Handle_Should_Return_Configuration_Dto()
    {
        var configId = new ConfigurationId(Guid.NewGuid());
        var config = Configuration.Create(configId, OwnerId, "Test Config", "Some description").Value;
        _configurationRepository.GetByIdAsync(configId, Arg.Any<CancellationToken>())
            .Returns(config);
        _moduleTemplateRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<ModuleTemplateId>>(), Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(new List<ModuleTemplate>());

        var handler = new GetConfigurationByIdHandler(_configurationRepository, _moduleTemplateRepository, _currentUserAccessor);
        var query = new GetConfigurationByIdQuery(configId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(configId.Value);
        result.Value.Title.Should().Be("Test Config");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Configuration_Not_Found()
    {
        var configId = new ConfigurationId(Guid.NewGuid());
        _configurationRepository.GetByIdAsync(configId, Arg.Any<CancellationToken>())
            .Returns((Configuration?)null);

        var handler = new GetConfigurationByIdHandler(_configurationRepository, _moduleTemplateRepository, _currentUserAccessor);
        var query = new GetConfigurationByIdQuery(configId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Configurations.NotFound");
    }
}
