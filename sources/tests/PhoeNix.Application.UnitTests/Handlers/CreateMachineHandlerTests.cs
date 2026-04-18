using FluentAssertions;
using NSubstitute;
using PhoeNix.Application.Machines.Commands;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.UnitTests.Handlers;

public class CreateMachineHandlerTests
{
    private readonly IMachineRepository _machineRepository = Substitute.For<IMachineRepository>();

    [Fact]
    public async Task Handle_Should_Create_Machine_And_Return_Id()
    {
        _machineRepository.GetByTitleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Machine?)null);
        _machineRepository.GetByMacAddressAsync(Arg.Any<System.Net.NetworkInformation.PhysicalAddress>(), Arg.Any<CancellationToken>())
            .Returns((Machine?)null);

        var handler = new CreateMachineHandler(_machineRepository);
        var command = new CreateMachineCommand(
            "My Machine",
            true,
            "AA:BB:CC:DD:EE:FF",
            Architecture.X86Linux,
            InstallDiskSelectionPreference.Biggest);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
        _machineRepository.Received(1).Add(Arg.Any<Machine>());
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Title_Already_Exists()
    {
        var existing = Machine.Create(new MachineId(Guid.NewGuid()), "AA:BB:CC:DD:EE:FF", "My Machine", true, Architecture.X86Linux, InstallDiskSelectionPreference.Biggest).Value;
        _machineRepository.GetByTitleAsync("My Machine", Arg.Any<CancellationToken>())
            .Returns(existing);

        var handler = new CreateMachineHandler(_machineRepository);
        var command = new CreateMachineCommand(
            "My Machine",
            true,
            "AA:BB:CC:DD:EE:FF",
            Architecture.X86Linux,
            InstallDiskSelectionPreference.Biggest);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Machines.TitleAlreadyExists");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Mac_Address_Invalid()
    {
        _machineRepository.GetByTitleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Machine?)null);

        var handler = new CreateMachineHandler(_machineRepository);
        var command = new CreateMachineCommand(
            "My Machine",
            true,
            "not-a-mac",
            Architecture.X86Linux,
            InstallDiskSelectionPreference.Biggest);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Machines.InvalidMacAddress");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_Mac_Address_Already_Exists()
    {
        _machineRepository.GetByTitleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Machine?)null);
        var existing = Machine.Create(new MachineId(Guid.NewGuid()), "AA:BB:CC:DD:EE:FF", "Other Machine", true, Architecture.X86Linux, InstallDiskSelectionPreference.Biggest).Value;
        _machineRepository.GetByMacAddressAsync(Arg.Any<System.Net.NetworkInformation.PhysicalAddress>(), Arg.Any<CancellationToken>())
            .Returns(existing);

        var handler = new CreateMachineHandler(_machineRepository);
        var command = new CreateMachineCommand(
            "My Machine",
            true,
            "AA:BB:CC:DD:EE:FF",
            Architecture.X86Linux,
            InstallDiskSelectionPreference.Biggest);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Machines.MacAddressAlreadyExists");
    }
}
