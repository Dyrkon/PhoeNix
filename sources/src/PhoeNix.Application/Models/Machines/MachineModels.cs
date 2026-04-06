using System.Net;
using FluentValidation;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Machines;

public record CreateMachineRequest(
    string Title,
    bool Enabled,
    string MacAddress,
    Architecture Architecture,
    InstallDiskSelectionPreference InstallDiskSelectionPreference);

public sealed record ListMachinesRequest(
    MachineSortField SortField = MachineSortField.Title,
    int Page = 1,
    int PageSize = 15,
    string? Search = null,
    bool? Enabled = null,
    Architecture? Architecture = null,
    MachineState? MachineState = null,
    SortDirection SortDirection = SortDirection.Ascending);

public record MachineListResponse(
    Guid Id,
    Guid? InstalledConfigurationId,
    string Title,
    bool Enabled,
    string MacAddress,
    Architecture Architecture,
    MachineState MachineState);

public record MachineDetailResponse(
    string Title,
    bool Enabled,
    string MacAddress,
    Architecture Architecture,
    InstallDiskSelectionPreference InstallDiskSelectionPreference,
    HardwareProfileResponse? HardwareProfile,
    SoftwareSnapshotResponse? SoftwareSnapshot,
    DeploymentSnapshotResponse? DeploymentSnapshot,
    MachineStatusResponse MachineStatus
);

public record HardwareProfileResponse(
    CpuProfileResponse CpuProfile,
    MemoryProfileResponse MemoryProfile,
    IReadOnlyList<GpuProfileResponse> GpuProfiles,
    IReadOnlyList<DiskProfileResponse> DiskProfiles,
    IReadOnlyList<PeripheralProfileResponse> PeripheralProfiles);

public record CpuProfileResponse(
    string? Vendor,
    string? Model,
    int? CoreCount,
    int? ThreadCount);

public record MemoryProfileResponse(
    long? TotalBytes,
    int? SlotCount,
    int? OccupiedSlotCount,
    IReadOnlyList<MemoryModuleProfileResponse> MemoryModuleProfiles);

public record MemoryModuleProfileResponse(string? Slot, long? SizeBytes);

public record GpuProfileResponse(string? Vendor, string? Model, long? VramBytes);

public record DiskProfileResponse(
    string? StableDevicePath,
    string? KernelDevicePath,
    string? Model,
    string? Vendor,
    string? BusType,
    long? SizeBytes,
    bool? IsRotational);

public record PeripheralProfileResponse(PeripheralKind PeripheralKind, string? Name, bool IsConnected);

public record SoftwareSnapshotResponse();

public record DeploymentSnapshotResponse(
    Guid ConfigurationId,
    string ConfigurationTitle,
    Guid SystemId,
    string SystemName,
    string IpAddress,
    DateTime ProvisionedAtUtc,
    IReadOnlyList<DeploymentDiskBindingResponse> DeploymentDiskBindings);

public record DeploymentDiskBindingResponse(int Index, string StableDevicePath);

public record MachineStatusResponse(
    MachineState MachineState,
    DateTime? LastProvisioned,
    DateTime? LastOrchestrated,
    DateTime? LastConfigured);

public sealed class CreateMachineRequestValidator : AbstractValidator<CreateMachineRequest>
{
    public CreateMachineRequestValidator()
    {
        // TODO
    }
}