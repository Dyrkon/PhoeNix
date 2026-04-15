using PhoeNix.Application.Models.Machines;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface IMetricsApiClient
{
    Task<ApiResult<MachineMetricsResponse>> GetMachineMetricsAsync(
        Guid machineId,
        string range = "24h",
        CancellationToken cancellationToken = default);
}
