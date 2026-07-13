using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.VmHosts.Queries;

public sealed record GetVmHostQuery(Guid VmHostId) : IQuery<VmHostDetailResponse>;

internal sealed class GetVmHostQueryHandler(
    IVmHostReadRepository vmHostReadRepository)
    : IQueryHandler<GetVmHostQuery, VmHostDetailResponse>
{
    public async Task<Result<VmHostDetailResponse>> Handle(
        GetVmHostQuery request,
        CancellationToken cancellationToken)
    {
        var host = await vmHostReadRepository.GetByIdAsync(request.VmHostId, cancellationToken);
        if (host is null)
            return Result.Failure<VmHostDetailResponse>(new Error("VmHosts.NotFound", "VM host not found."));

        return Result.Success(host);
    }
}
