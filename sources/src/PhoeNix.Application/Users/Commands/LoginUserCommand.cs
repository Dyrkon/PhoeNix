using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
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
    public Task<Result<AuthenticatedUserResponse>> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        return Result.Success(new UserLoginRequest(request.Name, request.Password))
            .Bind(Validate)
            .Bind(validated => GetUser(validated, cancellationToken)
                .Ensure(user => userPasswordHasher.VerifyPassword(user, validated.Password), UserErrors.InvalidCredentials)
                .Bind(async user =>
                {
                    await userSessionService.SignInAsync(user, cancellationToken);
                    return Result.Success(new AuthenticatedUserResponse(user.Id.Value, user.Name));
                }));
    }

    private static Result<UserLoginRequest> Validate(UserLoginRequest request)
    {
        return Result.Success(request)
            .Ensure(x => !string.IsNullOrWhiteSpace(x.Name), UserErrors.NameRequired)
            .Ensure(x => !string.IsNullOrWhiteSpace(x.Password), UserErrors.PasswordRequired);
    }

    private async Task<Result<User>> GetUser(UserLoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByNormalizedNameAsync(
            User.NormalizeName(request.Name),
            cancellationToken);

        return user is null
            ? Result.Failure<User>(UserErrors.InvalidCredentials)
            : Result.Success(user);
    }
}