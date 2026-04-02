using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Users.Commands;

public record RemoveSshKeyCommand(UserId UserId, string SshKey) : ICommand;

internal sealed class RemoveSshKeyCommandHandler(IUserRepository userRepository) : ICommandHandler<RemoveSshKeyCommand>
{
    public async Task<Result> Handle(RemoveSshKeyCommand request, CancellationToken cancellationToken)
    {
        return await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            .EnsureNotNull(new Error("UserNotFound", $"Cannot find user with id {request.UserId}"))
            .Bind(user => user.RemoveSshKey(request.SshKey));
    }
}