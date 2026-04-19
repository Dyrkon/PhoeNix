using FluentAssertions;
using NSubstitute;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Repositories;
using PhoeNix.Application.Users.Commands;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Application.UnitTests.Handlers;

public class RegisterUserHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUserPasswordHasher _passwordHasher = Substitute.For<IUserPasswordHasher>();
    private readonly IUserSessionService _sessionService = Substitute.For<IUserSessionService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    public RegisterUserHandlerTests()
    {
        _passwordHasher.HashPassword(Arg.Any<User>(), Arg.Any<string>())
            .Returns("hashed_password");
        _userRepository.ExistsByNormalizedNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(false);
    }

    [Fact]
    public async Task Handle_Should_Register_User_Successfully()
    {
        var handler = new RegisterUserCommandHandler(
            _userRepository, _passwordHasher, _sessionService, _unitOfWork);
        var command = new RegisterUserCommand("Alice", "SecurePassword123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Alice");
        await _sessionService.Received(1).SignInAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Name_Empty()
    {
        var handler = new RegisterUserCommandHandler(
            _userRepository, _passwordHasher, _sessionService, _unitOfWork);
        var command = new RegisterUserCommand("", "SecurePassword123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserNameRequired");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Name_Too_Short()
    {
        var handler = new RegisterUserCommandHandler(
            _userRepository, _passwordHasher, _sessionService, _unitOfWork);
        var command = new RegisterUserCommand("ab", "SecurePassword123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserNameLengthInvalid");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Password_Empty()
    {
        var handler = new RegisterUserCommandHandler(
            _userRepository, _passwordHasher, _sessionService, _unitOfWork);
        var command = new RegisterUserCommand("Alice", "");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserPasswordRequired");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Password_Too_Short()
    {
        var handler = new RegisterUserCommandHandler(
            _userRepository, _passwordHasher, _sessionService, _unitOfWork);
        var command = new RegisterUserCommand("Alice", "short");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserPasswordTooShort");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Name_Already_Taken()
    {
        _userRepository.ExistsByNormalizedNameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var handler = new RegisterUserCommandHandler(
            _userRepository, _passwordHasher, _sessionService, _unitOfWork);
        var command = new RegisterUserCommand("Alice", "SecurePassword123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserNameAlreadyTaken");
    }
}
