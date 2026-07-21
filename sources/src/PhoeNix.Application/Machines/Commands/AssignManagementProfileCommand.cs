using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.VmHosts;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Machines.Commands;

public record AssignManagementProfileCommand(
    Guid MachineId,
    Guid VmHostId,
    string ExternalId) : ICommand;

internal sealed class AssignManagementProfileHandler(
    IMachineRepository machineRepository,
    IVmHostRepository vmHostRepository)
    : ICommandHandler<AssignManagementProfileCommand>
{
    public async Task<Result> Handle(AssignManagementProfileCommand request, CancellationToken cancellationToken)
    {
        var machine = await machineRepository.GetByIdAsync(new MachineId(request.MachineId), cancellationToken);
        if (machine is null)
            return Result.Failure(new Error("Machines.NotFound", "Machine not found."));

        var vmHostId = new VmHostId(request.VmHostId);
        var vmHost = await vmHostRepository.GetByIdAsync(vmHostId, cancellationToken);
        if (vmHost is null)
            return Result.Failure(new Error("VmHosts.NotFound", "VM host not found."));

        return machine.AssignManagementProfile(vmHostId, request.ExternalId);
    }
}
