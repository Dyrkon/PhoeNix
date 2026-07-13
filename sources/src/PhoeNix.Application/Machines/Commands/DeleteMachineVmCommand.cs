using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Virtualization;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.VmHosts;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Machines.Commands;

public record DeleteMachineVmCommand(Guid MachineId) : ICommand;

internal sealed class DeleteMachineVmHandler(
    IMachineRepository machineRepository,
    IVmHostRepository vmHostRepository,
    IVirtualizationProviderFactory providerFactory)
    : ICommandHandler<DeleteMachineVmCommand>
{
    public async Task<Result> Handle(DeleteMachineVmCommand request, CancellationToken cancellationToken)
    {
        var machine = await machineRepository.GetByIdAsync(new MachineId(request.MachineId), cancellationToken);
        if (machine is null)
            return Result.Failure(new Error("Machines.NotFound", "Machine not found."));

        if (machine.ManagementProfile is null)
            return Result.Failure(new Error(
                "Machines.NoManagementProfile",
                $"Machine '{machine.Title}' does not have a management profile."));

        var vmHost = await vmHostRepository.GetByIdAsync(machine.ManagementProfile.VmHostId, cancellationToken);
        if (vmHost is null)
            return Result.Failure(new Error("VmHosts.NotFound", "Linked VM host not found."));

        var provider = providerFactory.GetProvider(vmHost.Provider);
        var deleteResult = await provider.DeleteVmAsync(
            vmHost.Credential, machine.ManagementProfile.ExternalId, cancellationToken);

        if (deleteResult.IsFailure)
            return deleteResult;

        return machine.ClearManagementProfile();
    }
}
