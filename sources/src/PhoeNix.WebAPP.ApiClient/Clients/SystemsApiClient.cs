using PhoeNix.Contracts.Validation;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public sealed class SystemsApiClient(HttpClient httpClient, IAuthenticationInvalidationNotifier notifier)
    : ApiClientBase(httpClient, notifier), ISystemsApiClient
{
    public Task<ApiResult> ScheduleSystemValidationAsync(
        Guid configurationId,
        Guid systemId,
        CancellationToken cancellationToken = default)
    {
        return PostAsync(
            $"validation/configurations/{configurationId}/systems/{systemId}",
            cancellationToken);
    }

    public Task<ApiResult<SystemValidationStatusResponse>> GetSystemValidationStatusAsync(
        Guid configurationId,
        Guid systemId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<SystemValidationStatusResponse>(
            $"validation/configurations/{configurationId}/systems/{systemId}/status",
            cancellationToken);
    }
}
