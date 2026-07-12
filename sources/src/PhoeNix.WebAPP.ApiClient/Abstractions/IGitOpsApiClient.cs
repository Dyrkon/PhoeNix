using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface IGitOpsApiClient
{
    Task<ApiResult> TriggerPushAsync(CancellationToken cancellationToken = default);
    Task<ApiResult> TriggerPullAsync(CancellationToken cancellationToken = default);
}
