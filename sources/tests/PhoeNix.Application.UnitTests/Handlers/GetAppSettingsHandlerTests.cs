using FluentAssertions;
using NSubstitute;
using PhoeNix.Application.Repositories;
using PhoeNix.Application.Settings.Queries;
using PhoeNix.Domain.Entities.AppSettings;

namespace PhoeNix.Application.UnitTests.Handlers;

public class GetAppSettingsHandlerTests
{
    private readonly IAppSettingsRepository _settingsRepository = Substitute.For<IAppSettingsRepository>();

    [Fact]
    public async Task Handle_Should_Return_Settings()
    {
        var settings = AppSettings.CreateDefault(new AppSettingsId(Guid.NewGuid()));
        _settingsRepository.GetAsync(Arg.Any<CancellationToken>()).Returns(settings);

        var handler = new GetAppSettingsQueryHandler(_settingsRepository);
        var result = await handler.Handle(new GetAppSettingsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FileStorageRootPath.Should().Be(settings.FileStorageRootPath);
        result.Value.SshCaKeyName.Should().Be(settings.SshCaKeyName);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Settings_Not_Initialized()
    {
        _settingsRepository.GetAsync(Arg.Any<CancellationToken>()).Returns((AppSettings?)null);

        var handler = new GetAppSettingsQueryHandler(_settingsRepository);
        var result = await handler.Handle(new GetAppSettingsQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AppSettings.NotFound");
    }
}
