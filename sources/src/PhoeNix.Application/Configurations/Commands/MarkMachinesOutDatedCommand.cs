using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public record MarkMachinesOutDated(ConfigurationId ConfigurationId) : ICommand;

internal sealed class MarkMachinesOutDatedHandler(IMachineRepository machineRepository)
    : ICommandHandler<MarkMachinesOutDated>
{
    public async Task<Result> Handle(MarkMachinesOutDated request, CancellationToken cancellationToken)
    {
        var machines = await machineRepository.GetAllByInstalledConfigurationIdAsync(
            request.ConfigurationId,
            cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var machine in machines)
            machine.ChangeMachineState(MachineState.OutDated, now);

        return Result.Success();
    }
}
