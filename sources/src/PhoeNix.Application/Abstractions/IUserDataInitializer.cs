using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Application.Abstractions;

public interface IUserDataInitializer
{
    Task InitializeForUserAsync(UserId userId, CancellationToken cancellationToken);
}
