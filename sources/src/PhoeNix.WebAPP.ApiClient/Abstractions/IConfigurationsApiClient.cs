using PhoeNix.Common.Models;
using PhoeNix.Contracts.Configurations;
using PhoeNix.Contracts.Inputs;
using PhoeNix.Contracts.Modules;
using PhoeNix.Contracts.Systems;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface IConfigurationsApiClient
{
    Task<ApiResult<PagedResponse<ConfigurationListResponse>>> GetConfigurationsAsync(
        ListConfigurationsRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<ConfigurationResponse>> GetConfigurationAsync(
        Guid configurationId,
        CancellationToken cancellationToken = default);

    Task<ApiResult<ConfigurationResponse>> CreateConfigurationAsync(
        CreateConfigurationRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<ModuleValueResponse>> AddConfigurationModuleAsync(
        Guid configurationId,
        CreateConfigurationModuleRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<ModuleValueResponse>> AddConfigurationSystemModuleAsync(
        Guid configurationId,
        Guid systemId,
        CreateConfigurationSystemModuleRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpdateConfigurationModuleAsync(
        Guid configurationId,
        Guid moduleValueId,
        UpdateConfigurationModuleRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpdateConfigurationSystemModuleAsync(
        Guid configurationId,
        Guid systemId,
        Guid moduleValueId,
        UpdateConfigurationSystemModuleRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<SystemResponse>> AddConfigurationSystemAsync(
        Guid configurationId,
        CreateConfigurationSystemRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpdateConfigurationSystemAsync(
        Guid configurationId,
        Guid systemId,
        UpdateConfigurationSystemRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<InputResponse>> AddConfigurationInputAsync(
        Guid configurationId,
        CreateConfigurationInputRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<InputResponse>> UpdateConfigurationInputAsync(
        Guid configurationId,
        Guid inputId,
        UpdateConfigurationInputRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> RemoveConfigurationInputAsync(
        Guid configurationId,
        Guid inputId,
        CancellationToken cancellationToken = default);

    Task<ApiResult<ConfigurationResponse>> ImportConfigurationAsync(
        ConfigurationResponse request,
        CancellationToken cancellationToken = default);
}
