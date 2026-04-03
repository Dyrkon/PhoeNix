using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Models.Machines;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Machines.Queries;

public sealed record GetMachineQuery(MachineId MachineId) : IQuery<MachineDetailResponse>;

internal sealed class GetMachineQueryHandler(
    IMachineReadRepository machineReadRepository)
    : IQueryHandler<GetMachineQuery, MachineDetailResponse>
{
    public async Task<Result<MachineDetailResponse>> Handle(
        GetMachineQuery request,
        CancellationToken cancellationToken)
    {
        return await machineReadRepository.GetByIdAsync(request.MachineId.Value, cancellationToken)
            .EnsureNotNull(MachineErrors.NotFound(request.MachineId));
    }
}