using PhoeNix.WebAPP.ApiClient.Contracts;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface IMachinesApiClient
{
    Task<ApiResult> CreateMachineAsync(
        CreateMachineRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<IEnumerable<MachineListResponse>>> GetMachinesAsync(CancellationToken cancellationToken = default);
}