using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Virtualization;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.VmHosts;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Machines.Commands;

public record PowerManageMachineCommand(Guid MachineId, PowerAction Action) : ICommand;

internal sealed class PowerManageMachineHandler(
    IMachineRepository machineRepository,
    IVmHostRepository vmHostRepository,
    IVirtualizationProviderFactory providerFactory)
    : ICommandHandler<PowerManageMachineCommand>
{
    public async Task<Result> Handle(PowerManageMachineCommand request, CancellationToken cancellationToken)
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
        var powerResult = await provider.PowerActionAsync(
            vmHost.Credential, machine.ManagementProfile.ExternalId, request.Action, cancellationToken);

        if (powerResult.IsFailure)
            return powerResult;

        var stateResult = await provider.GetPowerStateAsync(
            vmHost.Credential, machine.ManagementProfile.ExternalId, cancellationToken);

        if (stateResult.IsSuccess)
            machine.UpdatePowerState(stateResult.Value, DateTime.UtcNow);

        return Result.Success();
    }
}
