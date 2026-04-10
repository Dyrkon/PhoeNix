using PhoeNix.Application.Models.Configurations;
using PhoeNix.Common.Models;
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

    Task<ApiResult<Contracts.ModuleValueResponse>> AddConfigurationModuleAsync(
        Guid configurationId,
        Contracts.CreateConfigurationModuleRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<Contracts.ModuleValueResponse>> AddConfigurationSystemModuleAsync(
        Guid configurationId,
        Guid systemId,
        Contracts.CreateConfigurationSystemModuleRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpdateConfigurationModuleAsync(
        Guid configurationId,
        Guid moduleValueId,
        Contracts.UpdateConfigurationModuleRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpdateConfigurationSystemModuleAsync(
        Guid configurationId,
        Guid systemId,
        Guid moduleValueId,
        Contracts.UpdateConfigurationSystemModuleRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<Contracts.SystemResponse>> AddConfigurationSystemAsync(
        Guid configurationId,
        Contracts.CreateConfigurationSystemRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpdateConfigurationSystemAsync(
        Guid configurationId,
        Guid systemId,
        Contracts.UpdateConfigurationSystemRequest request,
        CancellationToken cancellationToken = default);
}