using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Machines;

public static class MachineErrors
{
    public static Error NotFound(MachineId machineId)
    {
        return new Error("Machines.NotFound", $"Machine '{machineId}' was not found.");
    }
}