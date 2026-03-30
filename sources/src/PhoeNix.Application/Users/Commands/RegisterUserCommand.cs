using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Models.Users;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Users.Commands;

public sealed record RegisterUserCommand(string Name, string Password)
    : ICommand<AuthenticatedUserResponse>, ISelfManagedUnitOfWorkCommand;

internal sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IUserPasswordHasher userPasswordHasher,
    IUserSessionService userSessionService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterUserCommand, AuthenticatedUserResponse>
{
    public async Task<Result<AuthenticatedUserResponse>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var validatedRequestResult = Result.Success(new RegisterUserRequest(request.Name, request.Password))
            .Bind(Validate);

        if (validatedRequestResult.IsFailure)
            return Result.Failure<AuthenticatedUserResponse>(validatedRequestResult.Error);

        var uniquenessResult = await EnsureNameIsUnique(validatedRequestResult.Value, cancellationToken);

        if (uniquenessResult.IsFailure)
            return Result.Failure<AuthenticatedUserResponse>(uniquenessResult.Error);

        var userResult = CreateUser(validatedRequestResult.Value)
            .Bind(user => SetPasswordHash(user, validatedRequestResult.Value.Password));

        if (userResult.IsFailure)
            return Result.Failure<AuthenticatedUserResponse>(userResult.Error);

        await userRepository.AddAsync(userResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await userSessionService.SignInAsync(userResult.Value, cancellationToken);

        return Result.Success(new AuthenticatedUserResponse(
            userResult.Value.Id.Value,
            userResult.Value.Name));
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
        var setPasswordResult = user.SetPasswordHash(passwordHash);

        return setPasswordResult.IsFailure
            ? Result.Failure<User>(setPasswordResult.Error)
            : Result.Success(user);
    }

    private sealed record RegisterUserRequest(string Name, string Password);
}