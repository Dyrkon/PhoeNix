using FluentAssertions;
using NSubstitute;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Repositories;
using PhoeNix.Application.Settings.Commands;
using PhoeNix.Domain.Entities.AppSettings;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.UnitTests.Handlers;

public class UpdateAppSettingsHandlerTests
{
    private static readonly UserId OwnerId = new(Guid.NewGuid());
    private readonly IAppSettingsRepository _settingsRepository = Substitute.For<IAppSettingsRepository>();
    private readonly ICurrentUserAccessor _currentUserAccessor = Substitute.For<ICurrentUserAccessor>();

    public UpdateAppSettingsHandlerTests()
    {
        _currentUserAccessor.GetUserId().Returns(Result.Success(OwnerId));
    }

    private static UpdateAppSettingsCommand BuildCommand(string fileStorageRoot = "/storage")
    {
        return new UpdateAppSettingsCommand(
            fileStorageRoot,
            "sshca-key", "principal", 24.0, "ed25519",
            "ed25519", "deployca-key", "deploy-principal", "deploy-user", 30.0,
            "ssh", "bootstrap", "probe-cmd", 5, 30, false,
            "nixos-install", "root", 60, false, false, false,
            "builder-host", false, false,
            "http://prometheus", 7.0, Domain.Enums.MonitoringAddressResolution.MdnsHostname, "lan",
            "http://netboot", "/netboot", "0.0.0.0", 8080);
    }

    [Fact]
    public async Task Handle_Should_Update_Settings()
    {
        var settings = AppSettings.CreateDefault(new AppSettingsId(Guid.NewGuid()), OwnerId);
        _settingsRepository.GetAsync(OwnerId, Arg.Any<CancellationToken>()).Returns(settings);

        var handler = new UpdateAppSettingsCommandHandler(_settingsRepository, _currentUserAccessor);
        var command = BuildCommand("/new-storage");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        settings.FileStorageRootPath.Should().Be("/new-storage");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Settings_Not_Initialized()
    {
        _settingsRepository.GetAsync(OwnerId, Arg.Any<CancellationToken>()).Returns((AppSettings?)null);

        var handler = new UpdateAppSettingsCommandHandler(_settingsRepository, _currentUserAccessor);
        var result = await handler.Handle(BuildCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AppSettings.NotFound");
    }
}