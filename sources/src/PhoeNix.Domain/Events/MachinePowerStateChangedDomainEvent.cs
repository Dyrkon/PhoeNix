using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Events;

public sealed record MachinePowerStateChangedDomainEvent(
    MachineId MachineId,
    VmPowerState NewPowerState) : IDomainEvent;
