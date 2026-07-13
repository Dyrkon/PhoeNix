using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Virtualization;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.VmHosts;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Machines.Commands;

public record CreateMachineVmCommand(
    Guid VmHostId,
    string Name,
    int CpuCores,
    int MemoryMb,
    int DiskSizeGb,
    string? NetworkBridge,
    Architecture Architecture,
    bool Enabled,
    InstallDiskSelectionPreference InstallDiskSelectionPreference) : ICommand<string>;

internal sealed class CreateMachineVmHandler(
    IMachineRepository machineRepository,
    IVmHostRepository vmHostRepository,
    IVirtualizationProviderFactory providerFactory,
    ICurrentUserAccessor currentUserAccessor)
    : ICommandHandler<CreateMachineVmCommand, string>
{
    public async Task<Result<string>> Handle(CreateMachineVmCommand request, CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<string>(userIdResult.Error);

        var vmHostId = new VmHostId(request.VmHostId);
        var vmHost = await vmHostRepository.GetByIdAsync(vmHostId, cancellationToken);
        if (vmHost is null)
            return Result.Failure<string>(new Error("VmHosts.NotFound", "VM host not found."));

        var existingMachine = await machineRepository.GetByTitleAsync(request.Name, cancellationToken);
        if (existingMachine is not null)
            return Result.Failure<string>(new Error(
                "Machines.TitleAlreadyExists",
                $"Machine with title '{request.Name}' already exists."));

        var provider = providerFactory.GetProvider(vmHost.Provider);
        var definition = new VmDefinition(
            request.Name, request.CpuCores, request.MemoryMb,
            request.DiskSizeGb, request.NetworkBridge, request.Architecture);

        var createResult = await provider.CreateVmAsync(vmHost.Credential, definition, cancellationToken);
        if (createResult.IsFailure)
            return Result.Failure<string>(createResult.Error);

        var vmInfo = createResult.Value;

        return Machine
            .Create(
                new MachineId(Guid.NewGuid()),
                userIdResult.Value,
                vmInfo.MacAddress,
                request.Name,
                request.Enabled,
                request.Architecture,
                request.InstallDiskSelectionPreference)
            .Tap(machine => machine.AssignManagementProfile(vmHostId, vmInfo.ExternalId))
            .Tap(machineRepository.Add)
            .Map(machine => machine.Id.Value.ToString());
    }
}
