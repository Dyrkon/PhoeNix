using PhoeNix.Contracts.VmHosts;
using PhoeNix.Domain.Entities.Machines;

namespace PhoeNix.Application.Mappings;

public static class MachineMapping
{
    public static MachineListResponse MapMachineToListDto(Machine machine)
    {
        return new MachineListResponse(
            machine.Id.Value,
            machine.DeploymentSnapshot?.ConfigurationId.Value,
            machine.Title,
            machine.Enabled,
            machine.MacAddress.ToString(),
            machine.Architecture,
            machine.MachineStatus.MachineState);
    }

    public static MachineDetailResponse MapMachineToDto(Machine machine, string? vmHostName = null)
    {
        return new MachineDetailResponse(
            machine.Title,
            machine.Enabled,
            machine.MacAddress.ToString(),
            machine.Architecture,
            machine.InstallDiskSelectionPreference,
            MapHardwareProfile(machine.HardwareProfile),
            MapSoftwareSnapshot(machine.SoftwareSnapshot),
            MapDeploymentSnapshot(machine.DeploymentSnapshot),
            MapMachineStatus(machine.MachineStatus),
            MapManagementProfile(machine.ManagementProfile, vmHostName));
    }

    private static ManagementProfileResponse? MapManagementProfile(ManagementProfile? profile, string? vmHostName)
    {
        if (profile is null)
            return null;

        return new ManagementProfileResponse(
            profile.VmHostId.Value,
            vmHostName ?? "Unknown",
            profile.ExternalId,
            profile.PowerState,
            profile.LastPowerStateCheckUtc);
    }

    private static HardwareProfileResponse? MapHardwareProfile(HardwareProfile? hardwareProfile)
    {
        if (hardwareProfile is null)
            return null;

        return new HardwareProfileResponse(
            MapCpuProfile(hardwareProfile.Cpu),
            MapMemoryProfile(hardwareProfile.Memory),
            hardwareProfile.Gpus.Select(MapGpuProfile).ToList(),
            hardwareProfile.Disks.Select(MapDiskProfile).ToList(),
            hardwareProfile.Peripherals.Select(MapPeripheralProfile).ToList());
    }

    private static CpuProfileResponse MapCpuProfile(CpuProfile? cpuProfile)
    {
        return new CpuProfileResponse(
            cpuProfile?.Vendor,
            cpuProfile?.Model,
            cpuProfile?.CoreCount,
            cpuProfile?.ThreadCount);
    }

    private static MemoryProfileResponse MapMemoryProfile(MemoryProfile? memoryProfile)
    {
        return new MemoryProfileResponse(
            memoryProfile?.TotalBytes,
            memoryProfile?.SlotCount,
            memoryProfile?.OccupiedSlotCount,
            memoryProfile?.Modules.Select(MapMemoryModuleProfile).ToList() ?? []);
    }

    private static MemoryModuleProfileResponse MapMemoryModuleProfile(MemoryModuleProfile memoryModuleProfile)
    {
        return new MemoryModuleProfileResponse(
            memoryModuleProfile.Slot,
            memoryModuleProfile.SizeBytes);
    }

    private static GpuProfileResponse MapGpuProfile(GpuProfile gpuProfile)
    {
        return new GpuProfileResponse(
            gpuProfile.Vendor,
            gpuProfile.Model,
            gpuProfile.VramBytes);
    }

    private static DiskProfileResponse MapDiskProfile(DiskProfile diskProfile)
    {
        return new DiskProfileResponse(
            diskProfile.StableDevicePath,
            diskProfile.KernelDevicePath,
            diskProfile.Model,
            diskProfile.Vendor,
            diskProfile.BusType,
            diskProfile.SizeBytes,
            diskProfile.IsRotational);
    }

    private static PeripheralProfileResponse MapPeripheralProfile(PeripheralProfile peripheralProfile)
    {
        return new PeripheralProfileResponse(
            peripheralProfile.Kind,
            peripheralProfile.Name,
            peripheralProfile.IsConnected);
    }

    private static SoftwareSnapshotResponse? MapSoftwareSnapshot(SoftwareSnapshot? softwareSnapshot)
    {
        return softwareSnapshot is null
            ? null
            : new SoftwareSnapshotResponse();
    }

    private static DeploymentSnapshotResponse? MapDeploymentSnapshot(DeploymentSnapshot? deploymentSnapshot)
    {
        if (deploymentSnapshot is null)
            return null;

        return new DeploymentSnapshotResponse(
            deploymentSnapshot.ConfigurationId.Value,
            deploymentSnapshot.ConfigurationTitle,
            deploymentSnapshot.SystemId.Value,
            deploymentSnapshot.SystemName,
            deploymentSnapshot.LastKnownIpAddress.ToString(),
            deploymentSnapshot.ProvisionedAtUtc,
            deploymentSnapshot.BoundDisks
                .OrderBy(binding => binding.Index)
                .Select(MapDeploymentDiskBinding)
                .ToList());
    }

    private static DeploymentDiskBindingResponse MapDeploymentDiskBinding(DeploymentDiskBinding binding)
    {
        return new DeploymentDiskBindingResponse(
            binding.Index,
            binding.StableDevicePath);
    }

    private static MachineStatusResponse MapMachineStatus(MachineStatus machineStatus)
    {
        return new MachineStatusResponse(
            machineStatus.MachineState,
            machineStatus.LastProvisioned,
            machineStatus.LastOrchestrated,
            machineStatus.LastConfigured);
    }
}