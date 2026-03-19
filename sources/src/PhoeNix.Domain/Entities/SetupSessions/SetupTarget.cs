using System.Net;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.SetupSessions;

public sealed class SetupTarget
{
    private readonly List<RankedDiskAssignment> _rankedDiskAssignments = [];

    private SetupTarget()
    {
    }

    public MachineId MachineId { get; private set; }

    public CallbackToken? CallbackToken { get; private set; }

    public SetupStage Stage { get; private set; }

    public IPAddress? IpAddress { get; private set; }

    public IReadOnlyCollection<RankedDiskAssignment> RankedDiskAssignments => _rankedDiskAssignments;

    public SystemId? SelectedSystemId { get; private set; }

    public ConfigurationId? SelectedConfigurationId { get; private set; }

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

    public Result AssignRankedDisks(IReadOnlyList<string> diskByIdPaths)
    {
        if (diskByIdPaths.Count == 0)
            return Result.Failure(new Error(
                "SetupTargetRankedDisksMissing",
                "At least one ranked disk assignment must be provided."));

        _rankedDiskAssignments.Clear();

        for (var i = 0; i < diskByIdPaths.Count; i++)
        {
            var assignmentResult = RankedDiskAssignment.Create(i, diskByIdPaths[i]);
            if (assignmentResult.IsFailure)
            {
                _rankedDiskAssignments.Clear();
                return assignmentResult.Error;
            }

            _rankedDiskAssignments.Add(assignmentResult.Value);
        }

        return Result.Success();
    }

    public Result ClearRankedDisks()
    {
        _rankedDiskAssignments.Clear();
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

    public static Result<SetupTarget> Create(MachineId machineId, SystemId systemId, ConfigurationId configurationId)
    {
        return Result.Success(new SetupTarget
        {
            MachineId = machineId,
            SelectedSystemId = systemId,
            SelectedConfigurationId = configurationId,
            Stage = SetupStage.Created
        });
    }
}