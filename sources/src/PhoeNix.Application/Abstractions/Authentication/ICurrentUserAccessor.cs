using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Authentication;

public interface ICurrentUserAccessor
{
    Result<UserId> GetUserId();
}