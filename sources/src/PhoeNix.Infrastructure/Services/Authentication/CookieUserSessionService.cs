using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Infrastructure.Services.Authentication;

public sealed class CookieUserSessionService(IHttpContextAccessor httpContextAccessor) : IUserSessionService
{
    public Task SignInAsync(User user, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext
                          ?? throw new InvalidOperationException(
                              "An active HttpContext is required to sign in a user.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.Value.ToString("D")),
            new(ClaimTypes.Name, user.Name)
        };

        var identity = new ClaimsIdentity(claims, AuthenticationSchemeNames.UserCookie);
        var principal = new ClaimsPrincipal(identity);

        return httpContext.SignInAsync(
            AuthenticationSchemeNames.UserCookie,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true
            });
    }

    public Task SignOutAsync(CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext
                          ?? throw new InvalidOperationException(
                              "An active HttpContext is required to sign out a user.");

        return httpContext.SignOutAsync(AuthenticationSchemeNames.UserCookie);
    }
}