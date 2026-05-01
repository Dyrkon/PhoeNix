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

        return await machineRepository.GetByIdAsync(request.MachineId, cancellationToken)
            .EnsureNotNull(MachineErrors.NotFound(request.MachineId))
            .Ensure(m => m.OwnerId == userIdResult.Value, MachineErrors.NotFound(request.MachineId))
            .Map(MachineMapping.MapMachineToDto);
    }
}