using System.Net;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.SetupSessions;

public sealed class SetupTarget
{
    private SetupTarget()
    {
    }

    public MachineId MachineId { get; private set; }
    public CallbackToken? CallbackToken { get; private set; }
    public SetupStage Stage { get; private set; }
    public IPAddress? IpAddress { get; private set; }

    public Result SetStage(SetupStage stage)
    {
        Stage = stage;
        return Result.Success();
    }

    public Result SetIpAddress(IPAddress ipAddress)
    {
        IpAddress = ipAddress;
        return Result.Success();
    }

    public Result RevokeCallbackToken(DateTime nowUtc)
    {
        if (CallbackToken is null)
            return Result.Failure(new Error("SetupTargetRevokeFail"));

        CallbackToken = CallbackToken with { RevokedAtUtc = nowUtc };
        return Result.Success();
    }

    public Result ClearCallbackToken()
    {
        CallbackToken = null;
        return Result.Success();
    }

    public Result AssignToken(CallbackToken callbackToken)
    {
        if (CallbackToken is not null)
            return Result.Failure(new Error("SetupTargetTokenAssignedAlready"));

        CallbackToken = callbackToken;
        return Result.Success();
    }

    public static Result<SetupTarget> Create(MachineId machineId)
    {
        return Result.Success(new SetupTarget
        {
            MachineId = machineId,
            Stage = SetupStage.Created
        });
    }
}