using PhoeNix.Common.Models;
using PhoeNix.Contracts.Machines;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface IMachinesApiClient
{
    Task<ApiResult> CreateMachineAsync(
        CreateMachineRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpdateMachineAsync(
        Guid machineId,
        UpdateMachineRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<PagedResponse<MachineListResponse>>> GetMachinesAsync(
        ListMachinesRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<MachineDetailResponse>> GetMachineAsync(
        Guid machineId,
        CancellationToken cancellationToken = default);
}
