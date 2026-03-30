using Microsoft.AspNetCore.Identity;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Infrastructure.Services.Authentication;

public sealed class AspNetUserPasswordHasher : IUserPasswordHasher
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}