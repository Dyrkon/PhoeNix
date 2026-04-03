using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Machines;
using PhoeNix.Application.Repositories;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Machines.Queries;

public sealed record ListMachinesQuery(ListMachinesRequest Request)
    : IQuery<PagedResponse<MachineListResponse>>;

internal sealed class ListMachinesQueryHandler(
    IMachineReadRepository machineReadRepository)
    : IQueryHandler<ListMachinesQuery, PagedResponse<MachineListResponse>>
{
    public async Task<Result<PagedResponse<MachineListResponse>>> Handle(
        ListMachinesQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Request.Page <= 0)
            return Result.Failure<PagedResponse<MachineListResponse>>(new Error(
                "Machines.InvalidPage",
                "Page must be greater than zero."));

        if (request.Request.PageSize <= 0)
            return Result.Failure<PagedResponse<MachineListResponse>>(new Error(
                "Machines.InvalidPageSize",
                "Page size must be greater than zero."));

        var response = await machineReadRepository.GetPageAsync(request.Request, cancellationToken);

        return Result.Success(response);
    }
}