using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Shared;

namespace PhoeNix.McpServer.Services;

internal sealed class McpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public Result<UserId> GetUserId()
    {
        var value = httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(value, out var userId))
            return Result.Failure<UserId>(new Error("UserUnauthenticated", "User is not authenticated."));

        return Result.Success(new UserId(userId));
    }
}
