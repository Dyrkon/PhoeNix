using System.Net;
using System.Net.NetworkInformation;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;
using Xunit.Abstractions;

namespace PhoeNix.Persistence.Tests;

public class MachineRepositoryTests : PersistenceTestsBase
{
    private static readonly UserId OwnerId = new(Guid.NewGuid());
    private IMachineRepository MachineRepository => ServiceProvider.GetRequiredService<IMachineRepository>();

    public MachineRepositoryTests(ITestOutputHelper output) : base(output)
    {
    }

    private static Machine CreateMachine(string title, string macAddress = "AA:BB:CC:DD:EE:FF")
    {
        return Machine.Create(
            new MachineId(Guid.NewGuid()),
            OwnerId,
            macAddress,
            title,
            true,
            Architecture.X86Linux,
            InstallDiskSelectionPreference.Biggest).Value;
    }

    [Fact]
    public async Task GetByTitleAsync_Should_Return_Machine_For_Exact_Title()
    {
        var machine = CreateMachine("test-machine");
        await PhoeNixDbContextSUT.Set<Machine>().AddAsync(machine);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var result = await MachineRepository.GetByTitleAsync("test-machine", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Title.Should().Be("test-machine");
    }

    [Fact]
    public async Task GetByTitleAsync_Should_Return_Machine_Case_Insensitive()
    {
        var machine = CreateMachine("case-machine");
        await PhoeNixDbContextSUT.Set<Machine>().AddAsync(machine);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var result = await MachineRepository.GetByTitleAsync("CASE-MACHINE", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Title.Should().Be("case-machine");
    }

    [Fact]
    public async Task GetByTitleAsync_Should_Return_Null_For_Whitespace()
    {
        var result = await MachineRepository.GetByTitleAsync("   ", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByTitleAsync_Should_Return_Null_For_Empty()
    {
        var result = await MachineRepository.GetByTitleAsync(string.Empty, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByMacAddressAsync_Should_Return_Machine_With_Matching_Mac()
    {
        var machine = CreateMachine("mac-machine", "11:22:33:44:55:66");
        await PhoeNixDbContextSUT.Set<Machine>().AddAsync(machine);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var mac = PhysicalAddress.Parse("11:22:33:44:55:66");
        var result = await MachineRepository.GetByMacAddressAsync(mac, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Title.Should().Be("mac-machine");
    }

    [Fact]
    public async Task GetByMacAddressAsync_Should_Return_Null_When_No_Match()
    {
        var mac = PhysicalAddress.Parse("FF:FF:FF:FF:FF:FF");
        var result = await MachineRepository.GetByMacAddressAsync(mac, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllByInstalledConfigurationIdAsync_Should_Return_Machines_With_Deployment_Snapshot()
    {
        var configId = new ConfigurationId(Guid.NewGuid());
        var systemId = new SystemId(Guid.NewGuid());
        var machineId = new MachineId(Guid.NewGuid());
        var machine = Machine.Create(
            machineId, OwnerId, "AA:BB:CC:DD:EE:01", "deployed-machine", true,
            Architecture.X86Linux, InstallDiskSelectionPreference.Biggest).Value;

        machine.RecordDeploymentSnapshot(
            configId, "Config Title",
            systemId, "System Name",
            IPAddress.Loopback,
            DateTime.UtcNow,
            new List<string> { "/dev/disk/by-id/disk0" });

        await PhoeNixDbContextSUT.Set<Machine>().AddAsync(machine);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var result = await MachineRepository.GetAllByInstalledConfigurationIdAsync(configId, CancellationToken.None);

        result.Should().ContainSingle(m => m.Id == machineId);
    }

    [Fact]
    public async Task GetAllByInstalledConfigurationIdAsync_Should_Not_Return_Machines_Without_Snapshot()
    {
        var configId = new ConfigurationId(Guid.NewGuid());
        var machine = CreateMachine("no-snapshot", "AA:BB:CC:DD:EE:02");
        await PhoeNixDbContextSUT.Set<Machine>().AddAsync(machine);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var result = await MachineRepository.GetAllByInstalledConfigurationIdAsync(configId, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllByInstalledConfigurationIdAsync_Should_Not_Return_Machines_With_Different_Config()
    {
        var configId = new ConfigurationId(Guid.NewGuid());
        var otherConfigId = new ConfigurationId(Guid.NewGuid());
        var systemId = new SystemId(Guid.NewGuid());
        var machine = Machine.Create(
            new MachineId(Guid.NewGuid()), OwnerId, "AA:BB:CC:DD:EE:03", "other-config", true,
            Architecture.X86Linux, InstallDiskSelectionPreference.Biggest).Value;

        machine.RecordDeploymentSnapshot(
            otherConfigId, "Other Config",
            systemId, "System",
            IPAddress.Loopback,
            DateTime.UtcNow,
            new List<string> { "/dev/disk/by-id/disk1" });

        await PhoeNixDbContextSUT.Set<Machine>().AddAsync(machine);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var result = await MachineRepository.GetAllByInstalledConfigurationIdAsync(configId, CancellationToken.None);

        result.Should().BeEmpty();
    }
}