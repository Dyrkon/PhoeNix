using System.Net;
using FluentAssertions;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.UnitTests;

public class MachineTests
{
    private static readonly UserId OwnerId = new(Guid.NewGuid());
    private readonly MachineId _machineId = new(Guid.NewGuid());
    private const string ValidMac = "AA:BB:CC:DD:EE:FF";

    private Machine CreateValid(bool enabled = true) =>
        Machine.Create(_machineId, OwnerId, ValidMac, "test-machine", enabled,
            Architecture.X86Linux, InstallDiskSelectionPreference.Biggest).Value;

    [Fact]
    public void Machine_Should_Create_Successfully()
    {
        var result = Machine.Create(_machineId, OwnerId, ValidMac, "test-machine", true,
            Architecture.X86Linux, InstallDiskSelectionPreference.Biggest);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(_machineId);
        result.Value.Title.Should().Be("test-machine");
        result.Value.Enabled.Should().BeTrue();
        result.Value.Architecture.Should().Be(Architecture.X86Linux);
        result.Value.InstallDiskSelectionPreference.Should().Be(InstallDiskSelectionPreference.Biggest);
        result.Value.HardwareProfile.Should().BeNull();
        result.Value.DeploymentSnapshot.Should().BeNull();
    }

    [Fact]
    public void Machine_Should_Fail_Create_With_Invalid_Mac()
    {
        var result = Machine.Create(_machineId, OwnerId, "not-a-mac", "test-machine", true,
            Architecture.X86Linux, InstallDiskSelectionPreference.Biggest);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Unable to parse machine MAC address");
    }

    [Fact]
    public void Machine_Should_ChangeMacAddress()
    {
        var machine = CreateValid();

        var result = machine.ChangeMacAddress("11:22:33:44:55:66");

        result.IsSuccess.Should().BeTrue();
        machine.MacAddress.ToString().Should().Be("112233445566");
    }

    [Fact]
    public void Machine_Should_Fail_ChangeMacAddress_With_Invalid()
    {
        var machine = CreateValid();

        var result = machine.ChangeMacAddress("invalid");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Unable to parse machine MAC address");
    }

    [Fact]
    public void Machine_Should_ChangeArchitecture()
    {
        var machine = CreateValid();

        var result = machine.ChangeArchitecture(Architecture.Aarch64Linux);

        result.IsSuccess.Should().BeTrue();
        machine.Architecture.Should().Be(Architecture.Aarch64Linux);
    }

    [Fact]
    public void Machine_Should_ChangeInstallDiskSelectionPreference()
    {
        var machine = CreateValid();

        var result = machine.ChangeInstallDiskSelectionPreference(InstallDiskSelectionPreference.Fastest);

        result.IsSuccess.Should().BeTrue();
        machine.InstallDiskSelectionPreference.Should().Be(InstallDiskSelectionPreference.Fastest);
    }

    [Fact]
    public void Machine_Should_RecordHardwareProfile()
    {
        var machine = CreateValid();
        var profile = HardwareProfile.Create(DateTime.UtcNow, null, null, null, null, null, null);

        var result = machine.RecordHardwareProfile(profile);

        result.IsSuccess.Should().BeTrue();
        machine.HardwareProfile.Should().NotBeNull();
    }

    [Fact]
    public void Machine_Should_ClearHardwareProfile()
    {
        var machine = CreateValid();
        machine.RecordHardwareProfile(HardwareProfile.Create(DateTime.UtcNow, null, null, null, null, null, null));

        var result = machine.ClearHardwareProfile();

        result.IsSuccess.Should().BeTrue();
        machine.HardwareProfile.Should().BeNull();
    }

    [Fact]
    public void Machine_Should_RecordDeploymentSnapshot()
    {
        var machine = CreateValid();
        var configId = new ConfigurationId(Guid.NewGuid());
        var systemId = new SystemId(Guid.NewGuid());
        var ip = IPAddress.Parse("192.168.1.1");

        var result = machine.RecordDeploymentSnapshot(configId, "MyConfig", systemId, "MySystem",
            ip, DateTime.UtcNow, new[] { "/dev/sda" });

        result.IsSuccess.Should().BeTrue();
        machine.DeploymentSnapshot.Should().NotBeNull();
        machine.DeploymentSnapshot!.ConfigurationId.Should().Be(configId);
        machine.DeploymentSnapshot.SystemId.Should().Be(systemId);
    }

    [Fact]
    public void Machine_Should_Enable()
    {
        var machine = CreateValid(enabled: false);

        var result = machine.Enable();

        result.IsSuccess.Should().BeTrue();
        machine.Enabled.Should().BeTrue();
    }

    [Fact]
    public void Machine_Should_Fail_Enable_When_Already_Enabled()
    {
        var machine = CreateValid(enabled: true);

        var result = machine.Enable();

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already enabled");
    }

    [Fact]
    public void Machine_Should_Disable()
    {
        var machine = CreateValid(enabled: true);

        var result = machine.Disable();

        result.IsSuccess.Should().BeTrue();
        machine.Enabled.Should().BeFalse();
    }

    [Fact]
    public void Machine_Should_Fail_Disable_When_Already_Disabled()
    {
        var machine = CreateValid(enabled: false);

        var result = machine.Disable();

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already disabled");
    }

    [Fact]
    public void Machine_Should_ChangeMachineState_Provisioned_From_Registered()
    {
        var machine = CreateValid();

        var result = machine.ChangeMachineState(MachineState.Provisioned, DateTime.UtcNow);

        result.IsSuccess.Should().BeTrue();
        machine.MachineStatus.MachineState.Should().Be(MachineState.Provisioned);
    }

    [Fact]
    public void Machine_Should_Fail_ChangeMachineState_When_Invalid_Transition()
    {
        var machine = CreateValid();

        var result = machine.ChangeMachineState(MachineState.Orchestrated, DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Can't go to machine state");
    }

    [Fact]
    public void Machine_Should_ChangeMachineState_Through_Valid_States()
    {
        var machine = CreateValid();
        var now = DateTime.UtcNow;

        machine.ChangeMachineState(MachineState.Provisioned, now).IsSuccess.Should().BeTrue();
        machine.ChangeMachineState(MachineState.Orchestrated, now).IsSuccess.Should().BeTrue();
        machine.ChangeMachineState(MachineState.Updated, now).IsSuccess.Should().BeTrue();

        machine.MachineStatus.MachineState.Should().Be(MachineState.Updated);
        machine.MachineStatus.LastProvisioned.Should().NotBeNull();
        machine.MachineStatus.LastOrchestrated.Should().NotBeNull();
        machine.MachineStatus.LastConfigured.Should().NotBeNull();
    }

    [Fact]
    public void Machine_Should_Allow_OutDated_From_Any_State()
    {
        var machine = CreateValid();

        var result = machine.ChangeMachineState(MachineState.OutDated, DateTime.UtcNow);

        result.IsSuccess.Should().BeTrue();
        machine.MachineStatus.MachineState.Should().Be(MachineState.OutDated);
    }

    [Fact]
    public void Machine_Should_Allow_Updated_From_OutDated()
    {
        var machine = CreateValid();
        var now = DateTime.UtcNow;
        machine.ChangeMachineState(MachineState.Provisioned, now);
        machine.ChangeMachineState(MachineState.Orchestrated, now);
        machine.ChangeMachineState(MachineState.OutDated, now);

        var result = machine.ChangeMachineState(MachineState.Updated, now);

        result.IsSuccess.Should().BeTrue();
        machine.MachineStatus.MachineState.Should().Be(MachineState.Updated);
    }
}
