namespace PhoeNix.Domain.Enums;

public enum MachineState
{
    Registered = 0,
    Provisioned = 1,
    Orchestrated = 2,
    Updated = 3,
    OutDated = 4,
    Decommissioned = 5,
    Failed = 6
}