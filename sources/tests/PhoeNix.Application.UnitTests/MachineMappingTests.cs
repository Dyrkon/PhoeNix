using FluentAssertions;
using PhoeNix.Application.Mappings;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.UnitTests;

public class MachineMappingTests
{
    private static readonly UserId OwnerId = new(Guid.NewGuid());

    private static Machine CreateMachine(string title = "TestMachine", string mac = "AA:BB:CC:DD:EE:FF") =>
        Machine.Create(
            new MachineId(Guid.NewGuid()),
            OwnerId,
            mac,
            title,
            true,
            Architecture.X86Linux,
            InstallDiskSelectionPreference.Biggest).Value;

    [Fact]
    public void MapMachineToListDto_Should_Map_Basic_Fields()
    {
        var machine = CreateMachine();

        var dto = MachineMapping.MapMachineToListDto(machine);

        dto.Id.Should().Be(machine.Id.Value);
        dto.Title.Should().Be("TestMachine");
        dto.Enabled.Should().BeTrue();
        dto.Architecture.Should().Be(Architecture.X86Linux);
        dto.MachineState.Should().Be(MachineState.Registered);
        dto.InstalledConfigurationId.Should().BeNull();
        dto.MacAddress.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void MapMachineToDto_Should_Map_Basic_Fields_Without_Profiles()
    {
        var machine = CreateMachine();

        var dto = MachineMapping.MapMachineToDto(machine);

        dto.Title.Should().Be("TestMachine");
        dto.Enabled.Should().BeTrue();
        dto.Architecture.Should().Be(Architecture.X86Linux);
        dto.InstallDiskSelectionPreference.Should().Be(InstallDiskSelectionPreference.Biggest);
        dto.HardwareProfile.Should().BeNull();
        dto.SoftwareSnapshot.Should().BeNull();
        dto.DeploymentSnapshot.Should().BeNull();
        dto.MachineStatus.MachineState.Should().Be(MachineState.Registered);
    }

    [Fact]
    public void MapMachineToDto_Should_Map_Hardware_Profile_When_Present()
    {
        var machine = CreateMachine();
        var hardwareProfile = HardwareProfile.Create(
            DateTime.UtcNow,
            CpuProfile.Create("Intel", "i7-12700", 12, 20),
            null,
            MemoryProfile.Create(32_000_000_000L, 4, 2, new[]
            {
                MemoryModuleProfile.Create("DIMM A1", 16_000_000_000L),
                MemoryModuleProfile.Create("DIMM A2", 16_000_000_000L)
            }),
            new[] { GpuProfile.Create("NVIDIA", "RTX 3080", 10_000_000_000L) },
            new[] { DiskProfile.Create("/dev/disk/by-id/nvme0", "/dev/nvme0", "Samsung 980", "Samsung", "NVMe", 1_000_000_000_000L, false) },
            Array.Empty<PeripheralProfile>());
        machine.RecordHardwareProfile(hardwareProfile);

        var dto = MachineMapping.MapMachineToDto(machine);

        dto.HardwareProfile.Should().NotBeNull();
        dto.HardwareProfile!.CpuProfile.Vendor.Should().Be("Intel");
        dto.HardwareProfile.CpuProfile.CoreCount.Should().Be(12);
        dto.HardwareProfile.MemoryProfile.TotalBytes.Should().Be(32_000_000_000L);
        dto.HardwareProfile.MemoryProfile.MemoryModuleProfiles.Should().HaveCount(2);
        dto.HardwareProfile.GpuProfiles.Should().ContainSingle(g => g.Model == "RTX 3080");
        dto.HardwareProfile.DiskProfiles.Should().ContainSingle(d => d.StableDevicePath == "/dev/disk/by-id/nvme0");
    }

    [Fact]
    public void MapMachineToDto_Should_Map_Memory_Profile_Without_Modules()
    {
        var machine = CreateMachine();
        var hardwareProfile = HardwareProfile.Create(
            DateTime.UtcNow,
            null, null,
            MemoryProfile.Create(8_000_000_000L, 2, 1, null),
            null, null, null);
        machine.RecordHardwareProfile(hardwareProfile);

        var dto = MachineMapping.MapMachineToDto(machine);

        dto.HardwareProfile!.MemoryProfile.TotalBytes.Should().Be(8_000_000_000L);
        dto.HardwareProfile.MemoryProfile.MemoryModuleProfiles.Should().BeEmpty();
    }
}
