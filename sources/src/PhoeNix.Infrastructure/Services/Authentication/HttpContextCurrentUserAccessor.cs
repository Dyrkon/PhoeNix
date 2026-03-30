using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.Authentication;

public sealed class HttpContextCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public Result<UserId> GetUserId()
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(value, out var userId))
            return Result.Failure<UserId>(new Error("UserUnauthenticated", "User is not authenticated."));

        return Result.Success(new UserId(userId));
    }
}