using PhoeNix.Application.Models.Settings;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface ISettingsApiClient
{
    Task<ApiResult<AppSettingsResponse>> GetSettingsAsync(CancellationToken cancellationToken = default);

    Task<ApiResult> UpdateSettingsAsync(
        UpdateAppSettingsRequest request,
        CancellationToken cancellationToken = default);
}
