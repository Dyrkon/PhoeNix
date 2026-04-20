using PhoeNix.Common.Models;
using PhoeNix.Contracts.Setup;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface ISetupApiClient
{
    Task<ApiResult<string>> StartSessionAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResult> StartMachineSetupAsync(
        Guid sessionId,
        Guid machineId,
        StartMachineSetupRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult<SetupStatusResponse>> GetSetupStatusAsync(
        Guid sessionId,
        Guid machineId,
        CancellationToken cancellationToken = default);

    Task<ApiResult<PagedResponse<SetupSessionListResponse>>> GetSessionsAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<ApiResult<SetupSessionDetailResponse>> GetSessionDetailAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task<ApiResult> CancelSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);
}
