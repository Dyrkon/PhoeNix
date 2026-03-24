using PhoeNix.Application.Models.Setup;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Setup;

public interface ISetupWorkflowDecider
{
    Result<SetupWorkflowDecision> Decide(SetupSession session, MachineId machineId);
}