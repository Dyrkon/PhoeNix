using FluentAssertions;
using NSubstitute;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Repositories;
using PhoeNix.Application.Users.Commands;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Application.UnitTests.Handlers;

public class LoginUserHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUserPasswordHasher _passwordHasher = Substitute.For<IUserPasswordHasher>();
    private readonly IUserSessionService _sessionService = Substitute.For<IUserSessionService>();

    private User CreateUser()
    {
        var user = User.Create(new UserId(Guid.NewGuid()), "alice").Value;
        user.SetPasswordHash("hashed");
        return user;
    }

    [Fact]
    public async Task Handle_Should_Login_Successfully()
    {
        var user = CreateUser();
        _userRepository.GetByNormalizedNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword(user, "password123").Returns(true);

        var handler = new LoginUserCommandHandler(_userRepository, _passwordHasher, _sessionService);
        var command = new LoginUserCommand("alice", "password123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("alice");
        await _sessionService.Received(1).SignInAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Name_Empty()
    {
        var handler = new LoginUserCommandHandler(_userRepository, _passwordHasher, _sessionService);
        var command = new LoginUserCommand("", "password123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserNameRequired");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Password_Empty()
    {
        var handler = new LoginUserCommandHandler(_userRepository, _passwordHasher, _sessionService);
        var command = new LoginUserCommand("alice", "");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserPasswordRequired");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Not_Found()
    {
        _userRepository.GetByNormalizedNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var handler = new LoginUserCommandHandler(_userRepository, _passwordHasher, _sessionService);
        var command = new LoginUserCommand("alice", "password123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserInvalidCredentials");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Password_Wrong()
    {
        var user = CreateUser();
        _userRepository.GetByNormalizedNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword(user, Arg.Any<string>()).Returns(false);

        var handler = new LoginUserCommandHandler(_userRepository, _passwordHasher, _sessionService);
        var command = new LoginUserCommand("alice", "wrongpassword");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserInvalidCredentials");
    }
}
