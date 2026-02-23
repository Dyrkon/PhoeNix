namespace PhoeNix.Domain.Shared;

public static class Timeouts
{
    public static readonly TimeSpan ProvisioningTtl = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan OrchestrationTtl = TimeSpan.FromMinutes(10);
}