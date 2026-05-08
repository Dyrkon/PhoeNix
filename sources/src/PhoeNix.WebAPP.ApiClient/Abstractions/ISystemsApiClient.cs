using PhoeNix.Contracts.Validation;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface ISystemsApiClient
{
    Task<ApiResult> ScheduleSystemValidationAsync(
        Guid configurationId,
        Guid systemId,
        CancellationToken cancellationToken = default);

    Task<ApiResult<SystemValidationStatusResponse>> GetSystemValidationStatusAsync(
        Guid configurationId,
        Guid systemId,
        CancellationToken cancellationToken = default);
}
