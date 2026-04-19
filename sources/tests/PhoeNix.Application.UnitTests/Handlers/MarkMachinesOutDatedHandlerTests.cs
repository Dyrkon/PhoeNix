using FluentAssertions;
using NSubstitute;
using PhoeNix.Application.Configurations.Commands;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.UnitTests.Handlers;

public class MarkMachinesOutDatedHandlerTests
{
    private readonly IMachineRepository _machineRepository = Substitute.For<IMachineRepository>();

    [Fact]
    public async Task Handle_Should_Mark_All_Machines_As_OutDated()
    {
        var configId = new ConfigurationId(Guid.NewGuid());
        var machine1 = Machine.Create(new MachineId(Guid.NewGuid()), "AA:BB:CC:DD:EE:FF", "M1", true, Architecture.X86Linux, InstallDiskSelectionPreference.Biggest).Value;
        var machine2 = Machine.Create(new MachineId(Guid.NewGuid()), "AA:BB:CC:DD:EE:FE", "M2", true, Architecture.X86Linux, InstallDiskSelectionPreference.Biggest).Value;
        _machineRepository.GetAllByInstalledConfigurationIdAsync(configId, Arg.Any<CancellationToken>())
            .Returns(new List<Machine> { machine1, machine2 });

        var handler = new MarkMachinesOutDatedHandler(_machineRepository);
        var command = new MarkMachinesOutDated(configId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        machine1.MachineStatus.MachineState.Should().Be(MachineState.OutDated);
        machine2.MachineStatus.MachineState.Should().Be(MachineState.OutDated);
    }

    [Fact]
    public async Task Handle_Should_Succeed_When_No_Machines()
    {
        var configId = new ConfigurationId(Guid.NewGuid());
        _machineRepository.GetAllByInstalledConfigurationIdAsync(configId, Arg.Any<CancellationToken>())
            .Returns(new List<Machine>());

        var handler = new MarkMachinesOutDatedHandler(_machineRepository);
        var result = await handler.Handle(new MarkMachinesOutDated(configId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
