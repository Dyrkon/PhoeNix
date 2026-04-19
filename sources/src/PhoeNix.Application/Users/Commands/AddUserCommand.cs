using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Users.Commands;

public record AddUserCommand(string Name) : ICommand<UserId>;

internal sealed class AddUserCommandHandler(IUserRepository userRepository) : ICommandHandler<AddUserCommand, UserId>
{
    public Task<Result<UserId>> Handle(AddUserCommand request, CancellationToken cancellationToken)
    {
        return User
            .Create(new UserId(Guid.NewGuid()), request.Name)
            .Tap(userRepository.Add)
            .Map(user => user.Id);
    }
}