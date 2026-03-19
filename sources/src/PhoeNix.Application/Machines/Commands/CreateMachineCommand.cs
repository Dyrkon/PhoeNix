using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
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
        return await Machine
            .Create(new MachineId(Guid.NewGuid()), request.MacAddress, request.Title, request.Enabled,
                request.Architecture, request.InstallDiskSelectionPreference)
            .Tap(machineRepository.Add)
            .Bind(machine => Task.FromResult(Result.Success(machine.Id.Value.ToString())));
    }
}