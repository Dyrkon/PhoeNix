using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.VmHosts;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.VmHosts.Commands;

public record RemoveVmHostCommand(Guid VmHostId) : ICommand;

internal sealed class RemoveVmHostHandler(
    IVmHostRepository vmHostRepository,
    IMachineRepository machineRepository)
    : ICommandHandler<RemoveVmHostCommand>
{
    public async Task<Result> Handle(RemoveVmHostCommand request, CancellationToken cancellationToken)
    {
        var vmHostId = new VmHostId(request.VmHostId);
        var vmHost = await vmHostRepository.GetByIdAsync(vmHostId, cancellationToken);
        if (vmHost is null)
            return Result.Failure(new Error("VmHosts.NotFound", "VM host not found."));

        var linkedMachines = await machineRepository.GetAllByVmHostIdAsync(vmHostId, cancellationToken);
        if (linkedMachines.Count > 0)
            return Result.Failure(new Error(
                "VmHosts.HasLinkedMachines",
                $"Cannot remove VM host '{vmHost.Name}' because {linkedMachines.Count} machine(s) still reference it. Clear their management profiles first."));

        vmHostRepository.Remove(vmHost);
        return Result.Success();
    }
}
