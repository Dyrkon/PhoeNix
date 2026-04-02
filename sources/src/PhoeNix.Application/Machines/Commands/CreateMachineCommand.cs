using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Machines.Commands;

public record CreateMachineCommand(
    string Title,
    bool Enabled,
    string MacAddress,
    Architecture Architecture,
    InstallDiskSelectionPreference InstallDiskSelectionPreference) : ICommand<string>;

internal sealed class CreateMachineHandler(IMachineRepository machineRepository)
    : ICommandHandler<CreateMachineCommand, string>
{
    public async Task<Result<string>> Handle(CreateMachineCommand request, CancellationToken cancellationToken)
    {
        var normalizedTitle = request.Title.Trim();

        var existingMachineByTitle = await machineRepository.GetByTitleAsync(normalizedTitle, cancellationToken);
        if (existingMachineByTitle is not null)
            return Result.Failure<string>(new Error(
                "Machines.TitleAlreadyExists",
                $"Machine with title '{normalizedTitle}' already exists."));

        if (!System.Net.NetworkInformation.PhysicalAddress.TryParse(request.MacAddress, out var macAddress))
            return Result.Failure<string>(new Error(
                "Machines.InvalidMacAddress",
                $"Unable to parse machine MAC address '{request.MacAddress}'."));

        var existingMachineByMacAddress = await machineRepository.GetByMacAddressAsync(macAddress, cancellationToken);
        if (existingMachineByMacAddress is not null)
            return Result.Failure<string>(new Error(
                "Machines.MacAddressAlreadyExists",
                $"Machine with MAC address '{request.MacAddress}' already exists."));

        return Machine
            .Create(
                new MachineId(Guid.NewGuid()),
                request.MacAddress,
                normalizedTitle,
                request.Enabled,
                request.Architecture,
                request.InstallDiskSelectionPreference)
            .Tap(machineRepository.Add)
            .Map(machine => machine.Id.Value.ToString());
    }
}