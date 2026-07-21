using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Machines.Queries;

public sealed record GetMachineQuery(MachineId MachineId) : IQuery<MachineDetailResponse>;

internal sealed class GetMachineQueryHandler(
    IMachineRepository machineRepository,
    IVmHostReadRepository vmHostReadRepository,
    ICurrentUserAccessor currentUserAccessor)
    : IQueryHandler<GetMachineQuery, MachineDetailResponse>
{
    public async Task<Result<MachineDetailResponse>> Handle(
        GetMachineQuery request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<MachineDetailResponse>(userIdResult.Error);

        var machineResult = await machineRepository.GetByIdAsync(request.MachineId, cancellationToken)
            .EnsureNotNull(MachineErrors.NotFound(request.MachineId))
            .Ensure(m => m.OwnerId == userIdResult.Value, MachineErrors.NotFound(request.MachineId));

        if (machineResult.IsFailure)
            return Result.Failure<MachineDetailResponse>(machineResult.Error);

        var machine = machineResult.Value;
        string? vmHostName = null;

        if (machine.ManagementProfile is not null)
        {
            var vmHost = await vmHostReadRepository.GetByIdAsync(
                machine.ManagementProfile.VmHostId.Value, cancellationToken);
            vmHostName = vmHost?.Name;
        }

        return MachineMapping.MapMachineToDto(machine, vmHostName);
    }
}