using PhoeNix.Contracts.VmHosts;
using PhoeNix.WebAPP.ApiClient.Models;

namespace PhoeNix.WebAPP.ApiClient.Abstractions;

public interface IVmHostsApiClient
{
    Task<ApiResult<IReadOnlyList<VmHostListResponse>>> ListVmHostsAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResult<VmHostDetailResponse>> GetVmHostAsync(
        Guid vmHostId,
        CancellationToken cancellationToken = default);

    Task<ApiResult> RegisterVmHostAsync(
        RegisterVmHostRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> UpdateVmHostAsync(
        Guid vmHostId,
        UpdateVmHostRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> RemoveVmHostAsync(
        Guid vmHostId,
        CancellationToken cancellationToken = default);

    Task<ApiResult> SyncResourcesAsync(
        Guid vmHostId,
        CancellationToken cancellationToken = default);

    Task<ApiResult> TestConnectionAsync(
        Guid vmHostId,
        CancellationToken cancellationToken = default);

    Task<ApiResult<IReadOnlyList<DiscoveredVmResponse>>> DiscoverVmsAsync(
        Guid vmHostId,
        CancellationToken cancellationToken = default);

    Task<ApiResult> AssignManagementProfileAsync(
        Guid machineId,
        AssignManagementProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> ClearManagementProfileAsync(
        Guid machineId,
        CancellationToken cancellationToken = default);

    Task<ApiResult> PowerManageAsync(
        Guid machineId,
        PowerManageRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> CreateMachineVmAsync(
        CreateMachineVmRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> DeleteMachineVmAsync(
        Guid machineId,
        CancellationToken cancellationToken = default);
}
