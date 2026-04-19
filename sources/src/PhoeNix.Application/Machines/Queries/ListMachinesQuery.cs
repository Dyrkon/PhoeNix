using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Machines;
using PhoeNix.Application.Repositories;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Machines.Queries;

public sealed record ListMachinesQuery(ListMachinesRequest Request)
    : IQuery<PagedResponse<MachineListResponse>>;

internal sealed class ListMachinesQueryHandler(
    IMachineReadRepository machineReadRepository)
    : IQueryHandler<ListMachinesQuery, PagedResponse<MachineListResponse>>
{
    public Task<Result<PagedResponse<MachineListResponse>>> Handle(
        ListMachinesQuery request,
        CancellationToken cancellationToken)
    {
        return Result.Success(request.Request)
            .Ensure(r => r.Page > 0, new Error("Machines.InvalidPage", "Page must be greater than zero."))
            .Ensure(r => r.PageSize > 0, new Error("Machines.InvalidPageSize", "Page size must be greater than zero."))
            .Map(r => machineReadRepository.GetPageAsync(r, cancellationToken));
    }
}