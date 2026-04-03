using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Users.Commands;

public record AddUserSshKeyCommand(UserId UserId, string SshKey) : ICommand;

internal sealed class AddUserSshKeyCommandHandler(IUserRepository userRepository)
    : ICommandHandler<AddUserSshKeyCommand>
{
    public async Task<Result> Handle(AddUserSshKeyCommand request, CancellationToken cancellationToken)
    {
        return await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            .EnsureNotNull(new Error("UserNotFound", $"Cannot find user with id {request.UserId}"))
            .Bind(user => user.AddSshKey(request.SshKey));
    }
}