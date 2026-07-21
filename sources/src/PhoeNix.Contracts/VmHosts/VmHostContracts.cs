using PhoeNix.Domain.Enums;

namespace PhoeNix.Contracts.VmHosts;

public record RegisterVmHostRequest(
    string Name,
    VmHostProvider Provider,
    string Host,
    int? Port,
    string? Username,
    string? Secret,
    string? ExtraConfig);

public record UpdateVmHostRequest(
    string Name,
    string Host,
    int? Port,
    string? Username,
    string? Secret,
    string? ExtraConfig);

public record VmHostListResponse(
    Guid Id,
    string Name,
    VmHostProvider Provider,
    bool Enabled,
    int LinkedMachineCount,
    VmHostResourcesResponse? Resources,
    DateTime? LastSyncedAtUtc);

public record VmHostDetailResponse(
    Guid Id,
    string Name,
    VmHostProvider Provider,
    bool Enabled,
    string Host,
    int? Port,
    string? Username,
    string? ExtraConfig,
    int LinkedMachineCount,
    VmHostResourcesResponse? Resources,
    DateTime? LastSyncedAtUtc);

public record VmHostResourcesResponse(
    int TotalCpuCores,
    int UsedCpuCores,
    long TotalMemoryMb,
    long UsedMemoryMb,
    long TotalStorageGb,
    long UsedStorageGb);

public record AssignManagementProfileRequest(
    Guid VmHostId,
    string ExternalId);

public record CreateMachineVmRequest(
    Guid VmHostId,
    string Name,
    int CpuCores,
    int MemoryMb,
    int DiskSizeGb,
    string? NetworkBridge,
    Architecture Architecture,
    bool Enabled,
    InstallDiskSelectionPreference InstallDiskSelectionPreference);

public record PowerManageRequest(PowerAction Action);

public record DiscoveredVmResponse(
    string ExternalId,
    string Name,
    int CpuCores,
    int MemoryMb,
    string? MacAddress,
    VmPowerState PowerState,
    Guid? LinkedMachineId);

public record ManagementProfileResponse(
    Guid VmHostId,
    string VmHostName,
    string ExternalId,
    VmPowerState PowerState,
    DateTime? LastPowerStateCheckUtc);
