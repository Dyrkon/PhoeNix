using PhoeNix.Common.Models;
using PhoeNix.Contracts.Configurations;
using PhoeNix.Contracts.Inputs;
using PhoeNix.Contracts.Modules;
using PhoeNix.Contracts.Systems;
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

    public async Task<ApiResult<ConfigurationWithRevisionsResponse>> GetConfigurationAsync(
        Guid configurationId,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<ConfigurationWithRevisionsResponse>(
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

    public async Task<ApiResult<ModuleValueResponse>> AddConfigurationModuleAsync(
        Guid configurationId,
        CreateConfigurationModuleRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostWithResponseAsync<ModuleValueResponse>(
            $"configurations/{configurationId}/modules",
            request,
            cancellationToken);
    }

    public async Task<ApiResult<ModuleValueResponse>> AddConfigurationSystemModuleAsync(
        Guid configurationId,
        Guid systemId,
        CreateConfigurationSystemModuleRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostWithResponseAsync<ModuleValueResponse>(
            $"configurations/{configurationId}/systems/{systemId}/modules",
            request,
            cancellationToken);
    }

    public async Task<ApiResult> UpdateConfigurationModuleAsync(
        Guid configurationId,
        Guid moduleValueId,
        UpdateConfigurationModuleRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PutAsync(
            $"configurations/{configurationId}/modules/{moduleValueId}",
            request,
            cancellationToken);
    }

    public async Task<ApiResult> UpdateConfigurationSystemModuleAsync(
        Guid configurationId,
        Guid systemId,
        Guid moduleValueId,
        UpdateConfigurationSystemModuleRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PutAsync(
            $"configurations/{configurationId}/systems/{systemId}/modules/{moduleValueId}",
            request,
            cancellationToken);
    }

    public async Task<ApiResult<SystemResponse>> AddConfigurationSystemAsync(
        Guid configurationId,
        CreateConfigurationSystemRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostWithResponseAsync<SystemResponse>(
            $"configurations/{configurationId}/systems",
            request,
            cancellationToken);
    }

    public async Task<ApiResult> UpdateConfigurationSystemAsync(
        Guid configurationId,
        Guid systemId,
        UpdateConfigurationSystemRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PutAsync(
            $"configurations/{configurationId}/systems/{systemId}",
            request,
            cancellationToken);
    }

    public async Task<ApiResult<InputResponse>> AddConfigurationInputAsync(
        Guid configurationId,
        CreateConfigurationInputRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostWithResponseAsync<InputResponse>(
            $"configurations/{configurationId}/inputs",
            request,
            cancellationToken);
    }

    public async Task<ApiResult<InputResponse>> UpdateConfigurationInputAsync(
        Guid configurationId,
        Guid inputId,
        UpdateConfigurationInputRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PutWithResponseAsync<InputResponse>(
            $"configurations/{configurationId}/inputs/{inputId}",
            request,
            cancellationToken);
    }

    public async Task<ApiResult> RemoveConfigurationInputAsync(
        Guid configurationId,
        Guid inputId,
        CancellationToken cancellationToken = default)
    {
        return await DeleteAsync(
            $"configurations/{configurationId}/inputs/{inputId}",
            cancellationToken);
    }
}