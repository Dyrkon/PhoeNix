namespace PhoeNix.Domain.Enums;

public enum SetupStage
{
    Created = 0,
    WaitingForPxe = 1,
    ArtefactsAssigned = 2,
    Bootstrapped = 3,
    Probed = 4,
    Orchestrated = 5,
    Finished = 6,
    Failed = 7,
    Cancelled = 8
}