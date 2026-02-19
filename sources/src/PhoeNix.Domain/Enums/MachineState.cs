namespace PhoeNix.Domain.Enums;

public enum MachineState
{
    Registered = 0,
    Provisioned = 1,
    Orchestrated = 2,
    Configured = 3,
    Decommissioned = 4,
    Failed = 5
}