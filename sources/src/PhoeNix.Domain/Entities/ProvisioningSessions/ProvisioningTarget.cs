using PhoeNix.Domain.Shared;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Entities.ProvisioningSessions;

public sealed record ProvisioningTarget
{
    private ProvisioningTarget()
    {
    }

    public MachineId MachineId { get; private set; } = default!;
    public CallbackToken CallbackToken { get; init; } = default!;
    public ProvisioningStage Stage { get; private set; }

    public ProvisioningTarget WithStage(ProvisioningStage stage)
    {
        return new ProvisioningTarget(MachineId, CallbackToken, stage);
    }

    public ProvisioningTarget RevokeCallbackToken(DateTime nowUtc)
    {
        return new ProvisioningTarget(MachineId, CallbackToken with { RevokedAtUtc = nowUtc }, Stage);
    }

    private ProvisioningTarget(MachineId machineId, CallbackToken callbackToken, ProvisioningStage stage)
    {
        MachineId = machineId;
        CallbackToken = callbackToken;
        Stage = stage;
    }

    public static Result<ProvisioningTarget> Create(MachineId machineId, CallbackToken callbackToken)
    {
        return Result.Success(new ProvisioningTarget(machineId, callbackToken, ProvisioningStage.Create));
    }
}