using PhoeNix.Domain.Shared;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Entities.ProvisioningSessions;

public sealed record ProvisioningTarget
{
    private ProvisioningTarget()
    {
    }

    public MachineId MachineId { get; private set; }
    public CallbackToken? CallbackToken { get; private set; }
    public ProvisioningStage Stage { get; private set; }

    public ProvisioningTarget WithStage(ProvisioningStage stage)
    {
        return new ProvisioningTarget { MachineId = MachineId, CallbackToken = CallbackToken, Stage = stage };
    }

    public Result RevokeCallbackToken(DateTime nowUtc)
    {
        if (CallbackToken != null)
        {
            CallbackToken = CallbackToken with { RevokedAtUtc = nowUtc };
            return Result.Success();
        }

        return Result.Failure(new Error("ProvisioningTargetRevokeFail"));
    }

    public Result AssignToken(CallbackToken callbackToken)
    {
        if (CallbackToken is not null)
            return Result.Failure(new Error("ProvisioningTargetTokenAssignedAlready"));
        CallbackToken = callbackToken;
        return Result.Success();
    }

    public static Result<ProvisioningTarget> Create(MachineId machineId)
    {
        return Result.Success(new ProvisioningTarget { MachineId = machineId, Stage = ProvisioningStage.Create });
    }
}