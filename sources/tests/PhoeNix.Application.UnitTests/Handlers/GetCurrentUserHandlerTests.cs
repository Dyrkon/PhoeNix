using FluentAssertions;
using NSubstitute;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Repositories;
using PhoeNix.Application.Users.Queries;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.UnitTests.Handlers;

public class GetCurrentUserHandlerTests
{
    private readonly ICurrentUserAccessor _currentUserAccessor = Substitute.For<ICurrentUserAccessor>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();

    [Fact]
    public async Task Handle_Should_Return_Current_User()
    {
        var userId = new UserId(Guid.NewGuid());
        var user = User.Create(userId, "alice").Value;
        _currentUserAccessor.GetUserId().Returns(Result.Success(userId));
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        var handler = new GetCurrentUserQueryHandler(_currentUserAccessor, _userRepository);
        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("alice");
        result.Value.Id.Should().Be(userId.Value);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Unauthenticated()
    {
        _currentUserAccessor.GetUserId()
            .Returns(Result.Failure<UserId>(new Error("UserUnauthenticated", "Not authenticated")));

        var handler = new GetCurrentUserQueryHandler(_currentUserAccessor, _userRepository);
        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserUnauthenticated");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Not_Found()
    {
        var userId = new UserId(Guid.NewGuid());
        _currentUserAccessor.GetUserId().Returns(Result.Success(userId));
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var handler = new GetCurrentUserQueryHandler(_currentUserAccessor, _userRepository);
        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserNotFound");
    }
}
