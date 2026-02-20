namespace PhoeNix.Domain.Enums;

public enum ProvisioningStage
{
    Create = 0,
    SecretsGenerated = 1,
    ArtefactsBuilt = 2,
    WaitingForPxe = 3,
    Bootstrapped = 4,
    Finished = 5,
    Failed = 6,
    Cancelled = 7
}