using PhoeNix.Application.Models.Configurations;
using PhoeNix.Common.Models;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Helpers;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public sealed class ConfigurationsApiClient(HttpClient httpClient, IAuthenticationInvalidationNotifier notifier)
    : ApiClientBase(httpClient, notifier), IConfigurationsApiClient
{
    public async Task<ApiResult<PagedResponse<ConfigurationListResponse>>> GetConfigurationsAsync(
        ListConfigurationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var queryString = QueryStringBuilder.BuildFrom(request);

        return await GetAsync<PagedResponse<ConfigurationListResponse>>(
            $"configurations{queryString}",
            cancellationToken);
    }

    public async Task<ApiResult<ConfigurationResponse>> GetConfigurationAsync(
        Guid configurationId,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<ConfigurationResponse>(
            $"configurations/{configurationId}",
            cancellationToken);
    }

    public async Task<ApiResult<ConfigurationResponse>> CreateConfigurationAsync(
        CreateConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostWithResponseAsync<ConfigurationResponse>(
            "configurations",
            request,
            cancellationToken);
    }
}