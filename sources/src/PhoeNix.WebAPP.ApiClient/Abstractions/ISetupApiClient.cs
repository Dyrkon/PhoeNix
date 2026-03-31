using PhoeNix.WebAPP.ApiClient.Contracts;
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

    Task<ApiResult> CancelSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);
}