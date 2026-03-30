using PhoeNix.Application.Abstractions.Setup;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.Setup;

internal sealed class SetupWorkflowDecider : ISetupWorkflowDecider
{
    public Result<SetupWorkflowDecision> Decide(
        SetupSession session,
        MachineId machineId)
    {
        var target = session.Targets.FirstOrDefault(t => t.MachineId == machineId);

        if (target is null)
            return Result.Failure<SetupWorkflowDecision>(new Error(
                "SetupTargetNotFound",
                $"Machine '{machineId.Value}' is not enrolled in setup session '{session.Id.Value}'."));

        return target.Stage switch
        {
            SetupStage.Created => Result.Success(
                SetupWorkflowDecision.Wait("Setup target has not been started yet.")),

            SetupStage.WaitingForPxe => Result.Success(
                SetupWorkflowDecision.Wait("Waiting for pixiecore boot request.")),

            SetupStage.ArtefactsAssigned => Result.Success(
                SetupWorkflowDecision.Wait("Waiting for bootstrap callback.")),

            SetupStage.Bootstrapped => Result.Success(
                SetupWorkflowDecision.ProbeHardware()),

            SetupStage.Probed => Result.Success(
                SetupWorkflowDecision.InstallMachine()),

            SetupStage.Orchestrated => Result.Success(
                SetupWorkflowDecision.Wait("Waiting for installed machine finalization callback.")),

            SetupStage.Finished => Result.Success(
                SetupWorkflowDecision.Wait("Setup has already finished.")),

            SetupStage.Failed => Result.Success(
                SetupWorkflowDecision.Wait("Setup has failed.")),

            SetupStage.Cancelled => Result.Success(
                SetupWorkflowDecision.Wait("Setup has been cancelled.")),

            _ => Result.Failure<SetupWorkflowDecision>(new Error(
                "SetupWorkflowStageUnsupported",
                $"Stage '{target.Stage}' is not supported by the setup workflow decider."))
        };
    }
}