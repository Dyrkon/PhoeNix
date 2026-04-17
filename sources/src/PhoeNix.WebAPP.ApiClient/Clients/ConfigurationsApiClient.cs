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

    public async Task<ApiResult<Contracts.ModuleValueResponse>> AddConfigurationModuleAsync(
        Guid configurationId,
        Contracts.CreateConfigurationModuleRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostWithResponseAsync<Contracts.ModuleValueResponse>(
            $"configurations/{configurationId}/modules",
            request,
            cancellationToken);
    }

    public async Task<ApiResult<Contracts.ModuleValueResponse>> AddConfigurationSystemModuleAsync(
        Guid configurationId,
        Guid systemId,
        Contracts.CreateConfigurationSystemModuleRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostWithResponseAsync<Contracts.ModuleValueResponse>(
            $"configurations/{configurationId}/systems/{systemId}/modules",
            request,
            cancellationToken);
    }

    public async Task<ApiResult> UpdateConfigurationModuleAsync(
        Guid configurationId,
        Guid moduleValueId,
        Contracts.UpdateConfigurationModuleRequest request,
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
        Contracts.UpdateConfigurationSystemModuleRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PutAsync(
            $"configurations/{configurationId}/systems/{systemId}/modules/{moduleValueId}",
            request,
            cancellationToken);
    }

    public async Task<ApiResult<Contracts.SystemResponse>> AddConfigurationSystemAsync(
        Guid configurationId,
        Contracts.CreateConfigurationSystemRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostWithResponseAsync<Contracts.SystemResponse>(
            $"configurations/{configurationId}/systems",
            request,
            cancellationToken);
    }

    public async Task<ApiResult> UpdateConfigurationSystemAsync(
        Guid configurationId,
        Guid systemId,
        Contracts.UpdateConfigurationSystemRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PutAsync(
            $"configurations/{configurationId}/systems/{systemId}",
            request,
            cancellationToken);
    }

    public async Task<ApiResult<Contracts.InputResponse>> AddConfigurationInputAsync(
        Guid configurationId,
        Contracts.CreateConfigurationInputRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostWithResponseAsync<Contracts.InputResponse>(
            $"configurations/{configurationId}/inputs",
            request,
            cancellationToken);
    }

    public async Task<ApiResult<Contracts.InputResponse>> UpdateConfigurationInputAsync(
        Guid configurationId,
        Guid inputId,
        Contracts.UpdateConfigurationInputRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PutWithResponseAsync<Contracts.InputResponse>(
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