using Humanizer;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Machines;

public class MachineStatus(MachineState machineState)
{
    public MachineState MachineState { get; private set; } = machineState;

    public DateTime LastContacted { get; private set; }

    public DateTime LastProvisioned { get; private set; }

    public DateTime LastOrchestrated { get; private set; }

    public DateTime LastConfigured { get; private set; }

    public Result ChangeMachineState(MachineState machineState)
    {
        switch (machineState)
        {
            case MachineState.Provisioned:
                if (MachineState != MachineState.Registered)
                    return Result.Failure(new Error("MachineStatusError",
                        $"Can't got to machine state {MachineState.Provisioned.Humanize()} from state {machineState.Humanize()}"));
                LastProvisioned = DateTime.Now;
                LastContacted = DateTime.Now;
                break;
            case MachineState.Orchestrated:
                if (MachineState != MachineState.Provisioned)
                    return Result.Failure(new Error("MachineStatusError",
                        $"Can't got to machine state {MachineState.Orchestrated.Humanize()} from state {machineState.Humanize()}"));
                LastOrchestrated = DateTime.Now;
                LastContacted = DateTime.Now;
                break;
            case MachineState.Configured:
                if (MachineState != MachineState.Orchestrated)
                    return Result.Failure(new Error("MachineStatusError",
                        $"Can't got to machine state {MachineState.Configured.Humanize()} from state {machineState.Humanize()}"));
                LastConfigured = DateTime.Now;
                LastContacted = DateTime.Now;
                break;
            case MachineState.Registered:
                LastContacted = DateTime.Now;
                break;
        }

        MachineState = machineState;
        return Result.Success();
    }
}