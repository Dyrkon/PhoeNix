using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Models.Users;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Users.Commands;

public sealed record LoginUserCommand(string Name, string Password) : ICommand<AuthenticatedUserResponse>;

internal sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    IUserPasswordHasher userPasswordHasher,
    IUserSessionService userSessionService)
    : ICommandHandler<LoginUserCommand, AuthenticatedUserResponse>
{
    public async Task<Result<AuthenticatedUserResponse>> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        var validatedRequestResult = Result.Success(new LoginRequest(request.Name, request.Password))
            .Bind(Validate);

        if (validatedRequestResult.IsFailure)
            return Result.Failure<AuthenticatedUserResponse>(validatedRequestResult.Error);

        var userResult = await GetUser(validatedRequestResult.Value, cancellationToken);

        if (userResult.IsFailure)
            return Result.Failure<AuthenticatedUserResponse>(userResult.Error);

        var passwordResult = Result.Success(userResult.Value)
            .Ensure(
                user => userPasswordHasher.VerifyPassword(user, validatedRequestResult.Value.Password),
                UserErrors.InvalidCredentials);

        if (passwordResult.IsFailure)
            return Result.Failure<AuthenticatedUserResponse>(passwordResult.Error);

        await userSessionService.SignInAsync(passwordResult.Value, cancellationToken);

        return Result.Success(new AuthenticatedUserResponse(
            passwordResult.Value.Id.Value,
            passwordResult.Value.Name));
    }

    private static Result<LoginRequest> Validate(LoginRequest request)
    {
        return Result.Success(request)
            .Ensure(x => !string.IsNullOrWhiteSpace(x.Name), UserErrors.NameRequired)
            .Ensure(x => !string.IsNullOrWhiteSpace(x.Password), UserErrors.PasswordRequired);
    }

    private async Task<Result<User>> GetUser(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByNormalizedNameAsync(
            User.NormalizeName(request.Name),
            cancellationToken);

        return user is null
            ? Result.Failure<User>(UserErrors.InvalidCredentials)
            : Result.Success(user);
    }
}