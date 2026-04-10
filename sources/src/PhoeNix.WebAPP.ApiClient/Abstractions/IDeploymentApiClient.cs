using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface IDeploymentApiClient
{
    Task<ApiResult> UpdateMachineAsync(
        Guid configurationId,
        Guid systemId,
        Guid machineId,
        CancellationToken cancellationToken = default);
}
