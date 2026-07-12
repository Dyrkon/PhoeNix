using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public sealed class GitOpsApiClient(HttpClient httpClient, IAuthenticationInvalidationNotifier notifier)
    : ApiClientBase(httpClient, notifier), IGitOpsApiClient
{
    public Task<ApiResult> TriggerPushAsync(CancellationToken cancellationToken = default)
    {
        return PostAsync("git-sync/push", cancellationToken);
    }

    public Task<ApiResult> TriggerPullAsync(CancellationToken cancellationToken = default)
    {
        return PostAsync("git-sync/pull", cancellationToken);
    }
}
