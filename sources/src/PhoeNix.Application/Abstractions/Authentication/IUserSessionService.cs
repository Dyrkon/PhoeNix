using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Application.Abstractions.Authentication;

public interface IUserSessionService
{
    Task SignInAsync(User user, CancellationToken cancellationToken);

    Task SignOutAsync(CancellationToken cancellationToken);
}