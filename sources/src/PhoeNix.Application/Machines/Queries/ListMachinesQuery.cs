using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Machines;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Machines.Queries;

public sealed record ListMachinesQuery() : IQuery<IEnumerable<MachineListResponse>>;

internal sealed class ListMachinesQueryHandler(IMachineRepository machineRepository)
    : IQueryHandler<ListMachinesQuery, IEnumerable<MachineListResponse>>
{
    public async Task<Result<IEnumerable<MachineListResponse>>> Handle(ListMachinesQuery request,
        CancellationToken cancellationToken)
    {
        var machines = await machineRepository.GetAllMachines(cancellationToken);

        return Result.Success<IEnumerable<MachineListResponse>>(machines.Select(MachineMapping.MapMachineToDto)
            .ToList());
    }
}