using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Machines.Commands;

public record ClearManagementProfileCommand(Guid MachineId) : ICommand;

internal sealed class ClearManagementProfileHandler(
    IMachineRepository machineRepository)
    : ICommandHandler<ClearManagementProfileCommand>
{
    public async Task<Result> Handle(ClearManagementProfileCommand request, CancellationToken cancellationToken)
    {
        var machine = await machineRepository.GetByIdAsync(new MachineId(request.MachineId), cancellationToken);
        if (machine is null)
            return Result.Failure(new Error("Machines.NotFound", "Machine not found."));

        return machine.ClearManagementProfile();
    }
}
