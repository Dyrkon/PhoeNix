using FluentAssertions;
using NSubstitute;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Repositories;
using PhoeNix.Application.Setup.Commands;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.UnitTests.Handlers;

public class StartSetupSessionHandlerTests
{
    private static readonly UserId OwnerId = new(Guid.NewGuid());
    private readonly ISetupSessionRepository _setupSessionRepository = Substitute.For<ISetupSessionRepository>();
    private readonly ISetupSshKeyProvider _sshKeyProvider = Substitute.For<ISetupSshKeyProvider>();
    private readonly ICurrentUserAccessor _currentUserAccessor = Substitute.For<ICurrentUserAccessor>();

    public StartSetupSessionHandlerTests()
    {
        _currentUserAccessor.GetUserId().Returns(Result.Success(OwnerId));
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Active_Session_Already_Exists()
    {
        _setupSessionRepository
            .HasActiveSessionAsync(OwnerId, Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new StartSetupSessionCommandHandler(_setupSessionRepository, _sshKeyProvider, _currentUserAccessor);
        var result = await handler.Handle(new StartSetupSessionCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("SetupSessions.ActiveSessionAlreadyExists");
        _setupSessionRepository.DidNotReceive().Add(Arg.Any<PhoeNix.Domain.Entities.SetupSessions.SetupSession>());
    }
}
