using Humanizer;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Machines;

public class MachineStatus(MachineState machineState)
{
    public MachineState MachineState { get; private set; } = machineState;

    public DateTime? LastContacted { get; private set; }

    public DateTime? LastProvisioned { get; private set; }

    public DateTime? LastOrchestrated { get; private set; }

    public DateTime? LastConfigured { get; private set; }

    public Result ChangeMachineState(MachineState machineState, DateTime now)
    {
        switch (machineState)
        {
            case MachineState.Provisioned:
                if (MachineState != MachineState.Registered)
                    return Result.Failure(new Error("MachineStatusError",
                        $"Can't go to machine state {machineState.Humanize()} from state {MachineState.Humanize()}"));
                LastProvisioned = now;
                LastContacted = now;
                break;
            case MachineState.Orchestrated:
                if (MachineState != MachineState.Provisioned)
                    return Result.Failure(new Error("MachineStatusError",
                        $"Can't go to machine state {machineState.Humanize()} from state {MachineState.Humanize()}"));
                LastOrchestrated = now;
                LastContacted = now;
                break;
            case MachineState.Updated:
                if (MachineState is not (MachineState.Orchestrated or MachineState.OutDated or MachineState.Updated))
                    return Result.Failure(new Error("MachineStatusError",
                        $"Can't go to machine state {machineState.Humanize()} from state {MachineState.Humanize()}"));
                LastConfigured = now;
                LastContacted = now;
                break;
            case MachineState.OutDated:
                break;
            case MachineState.Registered:
                LastContacted = now;
                break;
        }

        MachineState = machineState;
        return Result.Success();
    }
}