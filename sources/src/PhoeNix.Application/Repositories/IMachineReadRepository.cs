using PhoeNix.Common.Models;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Application.Repositories;

public interface IMachineReadRepository
{
    Task<PagedResponse<MachineListResponse>> GetPageAsync(
        ListMachinesRequest request,
        UserId ownerId,
        CancellationToken cancellationToken);

    Task<MachineDetailResponse?> GetByIdAsync(
        Guid machineId,
        CancellationToken cancellationToken);
}