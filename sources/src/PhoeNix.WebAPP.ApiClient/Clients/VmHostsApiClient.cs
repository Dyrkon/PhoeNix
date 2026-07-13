using PhoeNix.Contracts.VmHosts;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Clients;

public sealed class VmHostsApiClient(HttpClient httpClient, IAuthenticationInvalidationNotifier notifier)
    : ApiClientBase(httpClient, notifier), IVmHostsApiClient
{
    public async Task<ApiResult<IReadOnlyList<VmHostListResponse>>> ListVmHostsAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<IReadOnlyList<VmHostListResponse>>("vm-hosts", cancellationToken);
    }

    public async Task<ApiResult<VmHostDetailResponse>> GetVmHostAsync(
        Guid vmHostId,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<VmHostDetailResponse>($"vm-hosts/{vmHostId}", cancellationToken);
    }

    public async Task<ApiResult> RegisterVmHostAsync(
        RegisterVmHostRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync("vm-hosts", request, cancellationToken);
    }

    public async Task<ApiResult> UpdateVmHostAsync(
        Guid vmHostId,
        UpdateVmHostRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PutAsync($"vm-hosts/{vmHostId}", request, cancellationToken);
    }

    public async Task<ApiResult> RemoveVmHostAsync(
        Guid vmHostId,
        CancellationToken cancellationToken = default)
    {
        return await DeleteAsync($"vm-hosts/{vmHostId}", cancellationToken);
    }

    public async Task<ApiResult> SyncResourcesAsync(
        Guid vmHostId,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync($"vm-hosts/{vmHostId}/sync", cancellationToken);
    }

    public async Task<ApiResult> TestConnectionAsync(
        Guid vmHostId,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync($"vm-hosts/{vmHostId}/test-connection", cancellationToken);
    }

    public async Task<ApiResult<IReadOnlyList<DiscoveredVmResponse>>> DiscoverVmsAsync(
        Guid vmHostId,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<IReadOnlyList<DiscoveredVmResponse>>(
            $"vm-hosts/{vmHostId}/discover", cancellationToken);
    }

    public async Task<ApiResult> AssignManagementProfileAsync(
        Guid machineId,
        AssignManagementProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync($"machines/{machineId}/management-profile", request, cancellationToken);
    }

    public async Task<ApiResult> ClearManagementProfileAsync(
        Guid machineId,
        CancellationToken cancellationToken = default)
    {
        return await DeleteAsync($"machines/{machineId}/management-profile", cancellationToken);
    }

    public async Task<ApiResult> PowerManageAsync(
        Guid machineId,
        PowerManageRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync($"machines/{machineId}/power", request, cancellationToken);
    }

    public async Task<ApiResult> CreateMachineVmAsync(
        CreateMachineVmRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync("machines/create-vm", request, cancellationToken);
    }

    public async Task<ApiResult> DeleteMachineVmAsync(
        Guid machineId,
        CancellationToken cancellationToken = default)
    {
        return await DeleteAsync($"machines/{machineId}/vm", cancellationToken);
    }
}
