using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Models.Users;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Users.Queries;

public sealed record GetCurrentUserQuery() : IQuery<AuthenticatedUserResponse>;

internal sealed class GetCurrentUserQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository)
    : IQueryHandler<GetCurrentUserQuery, AuthenticatedUserResponse>
{
    public async Task<Result<AuthenticatedUserResponse>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();

        if (userIdResult.IsFailure)
            return Result.Failure<AuthenticatedUserResponse>(UserErrors.Unauthenticated);

        return await userRepository
            .GetByIdAsync(userIdResult.Value, cancellationToken)
            .EnsureNotNull(UserErrors.UserNotFound)
            .Map(user => new AuthenticatedUserResponse(user.Id.Value, user.Name));
    }
}