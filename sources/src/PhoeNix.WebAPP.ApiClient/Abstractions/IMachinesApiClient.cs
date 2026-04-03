using PhoeNix.Application.Models.Machines;
using PhoeNix.WebAPP.ApiClient.Contracts;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface IMachinesApiClient
{
    Task<ApiResult> CreateMachineAsync(
        CreateMachineRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<PagedResponse<MachineListResponse>>> GetMachinesAsync(
        ListMachinesRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<MachineDetailResponse>> GetMachineAsync(
        Guid machineId,
        CancellationToken cancellationToken = default);
}