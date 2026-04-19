using PhoeNix.Common.Models;
using PhoeNix.Contracts.Setup;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public sealed class SetupApiClient(HttpClient httpClient, IAuthenticationInvalidationNotifier notifier)
    : ApiClientBase(httpClient, notifier), ISetupApiClient
{
    public Task<ApiResult<string>> StartSessionAsync(
        CancellationToken cancellationToken = default)
    {
        return PostForValueAsync<string>("setup/session/start", cancellationToken);
    }

    public Task<ApiResult> StartMachineSetupAsync(
        Guid sessionId,
        Guid machineId,
        StartMachineSetupRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync(
            $"setup/session/{sessionId}/machine/{machineId}/start",
            request,
            cancellationToken);
    }

    public Task<ApiResult<SetupStatusResponse>> GetSetupStatusAsync(
        Guid sessionId,
        Guid machineId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<SetupStatusResponse>(
            $"setup/session/{sessionId}/machine/{machineId}/status",
            cancellationToken);
    }

    public Task<ApiResult<PagedResponse<SetupSessionListResponse>>> GetSessionsAsync(
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<PagedResponse<SetupSessionListResponse>>(
            $"setup/sessions?page={page}&pageSize={pageSize}",
            cancellationToken);
    }

    public Task<ApiResult<SetupSessionDetailResponse>> GetSessionDetailAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<SetupSessionDetailResponse>(
            $"setup/session/{sessionId}",
            cancellationToken);
    }

    public Task<ApiResult> CancelSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return PostAsync($"setup/session/{sessionId}/cancel", cancellationToken);
    }
}
