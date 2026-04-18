using PhoeNix.Application.Models.Settings;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public sealed class SettingsApiClient(HttpClient httpClient, IAuthenticationInvalidationNotifier notifier)
    : ApiClientBase(httpClient, notifier), ISettingsApiClient
{
    public Task<ApiResult<AppSettingsResponse>> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<AppSettingsResponse>("settings", cancellationToken);
    }

    public Task<ApiResult> UpdateSettingsAsync(
        UpdateAppSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        return PutAsync("settings", request, cancellationToken);
    }
}
