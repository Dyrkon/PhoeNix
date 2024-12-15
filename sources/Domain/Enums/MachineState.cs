namespace Domain.Enums;

public enum MachineState
{
    Failed,
    Inactive,
    Provisioning,
    ProvisioningDone,
    Configuring,
    ConfigurationDone,
    Done
}