using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Setup;

public sealed record SetupWorkflowDecision(
    SetupWorkflowAction Action,
    string Reason)
{
    public static SetupWorkflowDecision Wait(string reason)
    {
        return new SetupWorkflowDecision(SetupWorkflowAction.None, reason);
    }

    public static SetupWorkflowDecision ProbeHardware()
    {
        return new SetupWorkflowDecision(SetupWorkflowAction.ProbeHardware, string.Empty);
    }

    public static SetupWorkflowDecision InstallMachine()
    {
        return new SetupWorkflowDecision(SetupWorkflowAction.InstallMachine, string.Empty);
    }
}