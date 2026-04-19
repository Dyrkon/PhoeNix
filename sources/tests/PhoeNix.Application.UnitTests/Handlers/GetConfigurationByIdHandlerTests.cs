using FluentAssertions;
using NSubstitute;
using PhoeNix.Application.Configurations.Queries;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Application.UnitTests.Handlers;

public class GetConfigurationByIdHandlerTests
{
    private readonly IConfigurationRepository _configurationRepository =
        Substitute.For<IConfigurationRepository>();

    private readonly IModuleTemplateRepository _moduleTemplateRepository =
        Substitute.For<IModuleTemplateRepository>();

    [Fact]
    public async Task Handle_Should_Return_Configuration_Dto()
    {
        var configId = new ConfigurationId(Guid.NewGuid());
        var config = Configuration.Create(configId, "Test Config", "Some description").Value;
        _configurationRepository.GetByIdAsync(configId, Arg.Any<CancellationToken>())
            .Returns(config);
        _moduleTemplateRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<ModuleTemplateId>>(), Arg.Any<CancellationToken>())
            .Returns(new List<ModuleTemplate>());

        var handler = new GetConfigurationByIdHandler(_configurationRepository, _moduleTemplateRepository);
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

        var handler = new GetConfigurationByIdHandler(_configurationRepository, _moduleTemplateRepository);
        var query = new GetConfigurationByIdQuery(configId);

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Configurations.NotFound");
    }
}
