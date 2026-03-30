using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Users.Commands;

public sealed record LogoutUserCommand() : ICommand;

internal sealed class LogoutUserCommandHandler(IUserSessionService userSessionService)
    : ICommandHandler<LogoutUserCommand>
{
    public async Task<Result> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        await userSessionService.SignOutAsync(cancellationToken);
        return Result.Success();
    }
}