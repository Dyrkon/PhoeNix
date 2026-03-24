namespace PhoeNix.Application.Setup.Extensions;

using PhoeNix.Domain.Entities.Machines;
using Domain.Entities.SetupSessions;
using Domain.Enums;
using Domain.Shared;

internal static class SetupFailureTracking
{
    public static Result PersistFailure(
        this SetupSession session,
        MachineId machineId,
        Error error,
        string source,
        DateTime nowUtc,
        SetupStage? stage = null)
    {
        var recordResult = session.RecordMachineFailure(machineId, error, source, nowUtc);
        if (recordResult.IsFailure)
            return recordResult.Error;

        if (stage is null)
            return error;

        var stageResult = session.UpdateMachineStage(
            machineId,
            stage.Value,
            nowUtc,
            false);

        if (stageResult.IsFailure)
            return stageResult.Error;

        return error;
    }

    public static Result<T> PersistFailure<T>(
        this SetupSession session,
        MachineId machineId,
        Error error,
        string source,
        DateTime nowUtc,
        SetupStage? stage = null)
    {
        var result = session.PersistFailure(machineId, error, source, nowUtc, stage);
        return Result.Failure<T>(result.Error);
    }
}