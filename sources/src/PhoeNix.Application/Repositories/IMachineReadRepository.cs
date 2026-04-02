using PhoeNix.Application.Models.Machines;
using PhoeNix.Common.Models;

namespace PhoeNix.Application.Repositories;

public interface IMachineReadRepository
{
    Task<PagedResponse<MachineListResponse>> GetPageAsync(
        ListMachinesRequest request,
        CancellationToken cancellationToken);

    Task<MachineDetailResponse?> GetByIdAsync(
        Guid machineId,
        CancellationToken cancellationToken);
}