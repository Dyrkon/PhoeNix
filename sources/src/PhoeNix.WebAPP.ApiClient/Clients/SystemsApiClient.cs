using PhoeNix.Contracts.Systems;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public sealed class SystemsApiClient(HttpClient httpClient, IAuthenticationInvalidationNotifier notifier)
    : ApiClientBase(httpClient, notifier), ISystemsApiClient
{
    public Task<ApiResult<SystemTestResponse>> ValidateSystemAsync(
        Guid configurationId,
        Guid systemId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<SystemTestResponse>(
            $"systems/{configurationId}/system/{systemId}/validate",
            cancellationToken);
    }
}
