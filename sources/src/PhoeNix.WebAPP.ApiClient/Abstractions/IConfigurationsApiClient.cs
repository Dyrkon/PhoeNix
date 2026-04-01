using PhoeNix.WebAPP.ApiClient.Contracts;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface IConfigurationsApiClient
{
    Task<ApiResult<IReadOnlyList<ConfigurationListResponse>>> GetConfigurationsAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResult<ConfigurationResponse>> GetConfigurationByIdAsync(
        Guid configurationId,
        CancellationToken cancellationToken = default);

    Task<ApiResult> CreateConfigurationAsync(
        CreateConfigurationRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpdateConfigurationAsync(
        Guid configurationId,
        UpdateConfigurationRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<string>> BuildConfigurationAsync(
        Guid configurationId,
        CancellationToken cancellationToken = default);

    Task<ApiResult> AddConfigurationInputAsync(
        Guid configurationId,
        CreateConfigurationInputRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpdateConfigurationInputAsync(
        Guid configurationId,
        Guid inputId,
        UpdateConfigurationInputRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> AddConfigurationModuleAsync(
        Guid configurationId,
        CreateConfigurationModuleRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpdateConfigurationModuleAsync(
        Guid configurationId,
        Guid moduleValueId,
        UpdateConfigurationModuleRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> AddConfigurationSystemAsync(
        Guid configurationId,
        CreateConfigurationSystemRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpdateConfigurationSystemAsync(
        Guid configurationId,
        Guid systemId,
        UpdateConfigurationSystemRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> AddConfigurationSystemModuleAsync(
        Guid configurationId,
        Guid systemId,
        CreateConfigurationSystemModuleRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpdateConfigurationSystemModuleAsync(
        Guid configurationId,
        Guid systemId,
        Guid moduleValueId,
        UpdateConfigurationSystemModuleRequest request,
        CancellationToken cancellationToken = default);
}