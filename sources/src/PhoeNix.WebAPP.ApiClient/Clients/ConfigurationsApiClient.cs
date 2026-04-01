using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Contracts;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public sealed class ConfigurationsApiClient(HttpClient httpClient, IAuthenticationInvalidationNotifier notifier)
    : ApiClientBase(httpClient, notifier), IConfigurationsApiClient
{
    public Task<ApiResult<IReadOnlyList<ConfigurationListResponse>>> GetConfigurationsAsync(
        CancellationToken cancellationToken = default)
    {
        return GetAsync<IReadOnlyList<ConfigurationListResponse>>("configurations", cancellationToken);
    }

    public Task<ApiResult<ConfigurationResponse>> GetConfigurationByIdAsync(
        Guid configurationId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<ConfigurationResponse>($"configurations/{configurationId}", cancellationToken);
    }

    public Task<ApiResult> CreateConfigurationAsync(
        CreateConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync("configurations", request, cancellationToken);
    }

    public Task<ApiResult> UpdateConfigurationAsync(
        Guid configurationId,
        UpdateConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        return PutAsync($"configurations/{configurationId}", request, cancellationToken);
    }

    public Task<ApiResult<string>> BuildConfigurationAsync(
        Guid configurationId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<string>($"configurations/{configurationId}/build", cancellationToken);
    }

    public Task<ApiResult> AddConfigurationInputAsync(
        Guid configurationId,
        CreateConfigurationInputRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync($"configurations/{configurationId}/inputs", request, cancellationToken);
    }

    public Task<ApiResult> UpdateConfigurationInputAsync(
        Guid configurationId,
        Guid inputId,
        UpdateConfigurationInputRequest request,
        CancellationToken cancellationToken = default)
    {
        return PutAsync($"configurations/{configurationId}/inputs/{inputId}", request, cancellationToken);
    }

    public Task<ApiResult> AddConfigurationModuleAsync(
        Guid configurationId,
        CreateConfigurationModuleRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync($"configurations/{configurationId}/modules", request, cancellationToken);
    }

    public Task<ApiResult> UpdateConfigurationModuleAsync(
        Guid configurationId,
        Guid moduleValueId,
        UpdateConfigurationModuleRequest request,
        CancellationToken cancellationToken = default)
    {
        return PutAsync($"configurations/{configurationId}/modules/{moduleValueId}", request, cancellationToken);
    }

    public Task<ApiResult> AddConfigurationSystemAsync(
        Guid configurationId,
        CreateConfigurationSystemRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync($"configurations/{configurationId}/systems", request, cancellationToken);
    }

    public Task<ApiResult> UpdateConfigurationSystemAsync(
        Guid configurationId,
        Guid systemId,
        UpdateConfigurationSystemRequest request,
        CancellationToken cancellationToken = default)
    {
        return PutAsync($"configurations/{configurationId}/systems/{systemId}", request, cancellationToken);
    }

    public Task<ApiResult> AddConfigurationSystemModuleAsync(
        Guid configurationId,
        Guid systemId,
        CreateConfigurationSystemModuleRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync($"configurations/{configurationId}/systems/{systemId}/modules", request, cancellationToken);
    }

    public Task<ApiResult> UpdateConfigurationSystemModuleAsync(
        Guid configurationId,
        Guid systemId,
        Guid moduleValueId,
        UpdateConfigurationSystemModuleRequest request,
        CancellationToken cancellationToken = default)
    {
        return PutAsync(
            $"configurations/{configurationId}/systems/{systemId}/modules/{moduleValueId}",
            request,
            cancellationToken);
    }
}