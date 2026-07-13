using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.VmHosts.Queries;

public sealed record ListVmHostsQuery : IQuery<IReadOnlyList<VmHostListResponse>>;

internal sealed class ListVmHostsQueryHandler(
    IVmHostReadRepository vmHostReadRepository,
    ICurrentUserAccessor currentUserAccessor)
    : IQueryHandler<ListVmHostsQuery, IReadOnlyList<VmHostListResponse>>
{
    public async Task<Result<IReadOnlyList<VmHostListResponse>>> Handle(
        ListVmHostsQuery request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<IReadOnlyList<VmHostListResponse>>(userIdResult.Error);

        var hosts = await vmHostReadRepository.GetAllAsync(userIdResult.Value, cancellationToken);
        return Result.Success(hosts);
    }
}
