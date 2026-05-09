using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Machines.Commands;

public record UpdateMachineCommand(
    MachineId MachineId,
    string Title,
    bool Enabled,
    string MacAddress,
    Architecture Architecture,
    InstallDiskSelectionPreference InstallDiskSelectionPreference) : ICommand;

internal sealed class UpdateMachineCommandHandler(IMachineRepository machineRepository)
    : ICommandHandler<UpdateMachineCommand>
{
    public async Task<Result> Handle(UpdateMachineCommand request, CancellationToken cancellationToken)
    {
        var machine = await machineRepository.GetByIdAsync(request.MachineId, cancellationToken);
        if (machine is null)
            return Result.Failure(MachineErrors.NotFound(request.MachineId));
        var now = DateTime.UtcNow;

        if (!string.Equals(machine.Title, request.Title, StringComparison.Ordinal))
        {
            var existing = await machineRepository.GetByTitleAsync(request.Title, cancellationToken);
            if (existing is not null)
                return Result.Failure(new Error(
                    "Machines.TitleAlreadyExists",
                    $"Machine with title '{request.Title}' already exists."));
        }

        if (!string.Equals(machine.MacAddress.ToString(), request.MacAddress, StringComparison.OrdinalIgnoreCase))
        {
            if (System.Net.NetworkInformation.PhysicalAddress.TryParse(request.MacAddress, out var parsedMac))
            {
                var existing = await machineRepository.GetByMacAddressAsync(parsedMac, cancellationToken);
                if (existing is not null && existing.Id != machine.Id)
                    return Result.Failure(new Error(
                        "Machines.MacAddressAlreadyExists",
                        $"Machine with MAC address '{request.MacAddress}' already exists."));
            }
        }

        var titleResult = machine.ChangeTitle(request.Title, now);
        if (titleResult.IsFailure) return titleResult.Error;

        var macResult = machine.ChangeMacAddress(request.MacAddress);
        if (macResult.IsFailure) return macResult.Error;

        machine.ChangeArchitecture(request.Architecture);
        machine.ChangeInstallDiskSelectionPreference(request.InstallDiskSelectionPreference);

        if (request.Enabled && !machine.Enabled)
            machine.Enable();
        else if (!request.Enabled && machine.Enabled)
            machine.Disable();

        return Result.Success();
    }
}