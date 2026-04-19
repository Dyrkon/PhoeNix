using PhoeNix.Contracts.Systems;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface ISystemsApiClient
{
    Task<ApiResult<SystemTestResponse>> ValidateSystemAsync(
        Guid configurationId,
        Guid systemId,
        CancellationToken cancellationToken = default);
}
