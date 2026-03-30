using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Application.Abstractions.Authentication;

public interface IUserPasswordHasher
{
    string HashPassword(User user, string password);

    bool VerifyPassword(User user, string password);
}