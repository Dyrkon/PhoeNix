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

    public string? SelectedInstallDiskByIdPath { get; private set; }

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

    public Result AssignSelectedInstallDisk(string diskPath)
    {
        if (string.IsNullOrWhiteSpace(diskPath))
            return Result.Failure(new Error(
                "SetupTargetSelectedInstallDiskInvalid",
                "Selected install disk path cannot be empty."));

        if (!diskPath.StartsWith("/dev/disk/by-id", StringComparison.Ordinal))
            return Result.Failure(new Error(
                "SetupTargetSelectedInstallDiskInvalid",
                $"Selected install disk path '{diskPath}' must be an stable /dev/disk/by-id/... path."));

        SelectedInstallDiskByIdPath = diskPath.Trim();
        return Result.Success();
    }

    public Result ClearSelectedInstallDisk()
    {
        SelectedInstallDiskByIdPath = null;
        return Result.Success();
    }

    public Result RevokeCallbackToken(DateTime nowUtc)
    {
        if (CallbackToken is null)
            return Result.Failure(new Error(
                "SetupTargetCallbackTokenMissing",
                "No callback token is assigned to the setup target."));

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
            return Result.Failure(new Error(
                "SetupTargetTokenAlreadyAssigned",
                "A callback token is already assigned to the setup target."));

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