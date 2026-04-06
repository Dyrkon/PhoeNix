using PhoeNix.Application.Models.Setup;
using PhoeNix.Common.Models;
using PhoeNix.WebAPP.ApiClient.Contracts;
using PhoeNix.WebAPP.ApiClient.Models;
using SetupStatusResponse = PhoeNix.Application.Models.Setup.SetupStatusResponse;
using StartMachineSetupRequest = PhoeNix.Application.Models.Setup.StartMachineSetupRequest;

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

    Task<ApiResult<Common.Models.PagedResponse<SetupSessionListResponse>>> GetSessionsAsync(
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