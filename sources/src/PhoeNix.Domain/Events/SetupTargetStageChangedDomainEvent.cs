using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Events;

public sealed record SetupTargetStageChangedDomainEvent(
    SetupSessionId SessionId,
    MachineId MachineId,
    SetupStage PreviousStage,
    SetupStage CurrentStage) : IDomainEvent;