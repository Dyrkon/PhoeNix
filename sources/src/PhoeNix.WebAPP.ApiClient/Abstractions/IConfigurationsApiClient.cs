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
}