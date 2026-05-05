using PhoeNix.Application.Abstractions;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Users.Commands;

public sealed record RegisterUserCommand(string Name, string Password)
    : ICommand<AuthenticatedUserResponse>, ISelfManagedUnitOfWorkCommand;

internal sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IUserPasswordHasher userPasswordHasher,
    IUserSessionService userSessionService,
    IUserDataInitializer userDataInitializer,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterUserCommand, AuthenticatedUserResponse>
{
    public Task<Result<AuthenticatedUserResponse>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        return Result.Success(new RegisterUserRequest(request.Name, request.Password))
            .Bind(Validate)
            .Bind(validated => EnsureNameIsUnique(validated, cancellationToken))
            .Bind(validated => CreateUser(validated).Bind(user => SetPasswordHash(user, validated.Password)))
            .Bind(async user =>
            {
                await userRepository.AddAsync(user, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await userDataInitializer.InitializeForUserAsync(user.Id, cancellationToken);
                await userSessionService.SignInAsync(user, cancellationToken);
                return Result.Success(new AuthenticatedUserResponse(user.Id.Value, user.Name));
            });
    }

    private static Result<RegisterUserRequest> Validate(RegisterUserRequest request)
    {
        return Result.Success(request)
            .Ensure(x => !string.IsNullOrWhiteSpace(x.Name), UserErrors.NameRequired)
            .Ensure(x => x.Name.Trim().Length is >= 3 and <= 64, UserErrors.NameLengthInvalid)
            .Ensure(x => !string.IsNullOrWhiteSpace(x.Password), UserErrors.PasswordRequired)
            .Ensure(x => x.Password.Length >= 8, UserErrors.PasswordTooShort);
    }

    private async Task<Result<RegisterUserRequest>> EnsureNameIsUnique(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var exists = await userRepository.ExistsByNormalizedNameAsync(
            User.NormalizeName(request.Name),
            cancellationToken);

        return exists
            ? Result.Failure<RegisterUserRequest>(UserErrors.NameAlreadyTaken)
            : Result.Success(request);
    }

    private static Result<User> CreateUser(RegisterUserRequest request)
    {
        return User.Create(new UserId(Guid.NewGuid()), request.Name);
    }

    private Result<User> SetPasswordHash(User user, string password)
    {
        var passwordHash = userPasswordHasher.HashPassword(user, password);
        return user.SetPasswordHash(passwordHash).Map(() => user);
    }

    private sealed record RegisterUserRequest(string Name, string Password);
}