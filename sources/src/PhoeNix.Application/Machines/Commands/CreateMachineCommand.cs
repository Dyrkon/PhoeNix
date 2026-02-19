using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Machines.Commands;

public record CreateMachineCommand(string Title, bool Enabled, string MacAddress) : ICommand;

internal sealed class CreateMachineHandler(IMachineRepository machineRepository) : ICommandHandler<CreateMachineCommand>
{
    public Task<Result> Handle(CreateMachineCommand request, CancellationToken cancellationToken)
    {
        return Machine
            .Create(new MachineId(Guid.NewGuid()), request.MacAddress, request.Title, request.Enabled)
            .Tap(machineRepository.Add);
    }
}