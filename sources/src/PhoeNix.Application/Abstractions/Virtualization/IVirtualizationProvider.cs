using PhoeNix.Domain.Entities.VmHosts;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Virtualization;

public interface IVirtualizationProvider
{
    VmHostProvider ProviderType { get; }

    Task<Result> TestConnectionAsync(VmHostCredential credential, CancellationToken ct);

    Task<Result<VmHostResources>> GetResourcesAsync(VmHostCredential credential, CancellationToken ct);

    Task<Result<CreatedVmInfo>> CreateVmAsync(
        VmHostCredential credential, VmDefinition definition, CancellationToken ct);

    Task<Result> DeleteVmAsync(VmHostCredential credential, string externalId, CancellationToken ct);

    Task<Result> PowerActionAsync(
        VmHostCredential credential, string externalId, PowerAction action, CancellationToken ct);

    Task<Result<VmPowerState>> GetPowerStateAsync(
        VmHostCredential credential, string externalId, CancellationToken ct);

    Task<Result<IReadOnlyList<DiscoveredVm>>> ListVmsAsync(
        VmHostCredential credential, CancellationToken ct);
}

public record VmDefinition(
    string Name,
    int CpuCores,
    int MemoryMb,
    int DiskSizeGb,
    string? NetworkBridge,
    Architecture Architecture);

public record CreatedVmInfo(string ExternalId, string Name, string MacAddress);

public record DiscoveredVm(
    string ExternalId,
    string Name,
    int CpuCores,
    int MemoryMb,
    string? MacAddress,
    VmPowerState PowerState);
