using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.VmHosts;
using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Events;

public sealed record ManagementProfileAssignedDomainEvent(
    MachineId MachineId,
    VmHostId VmHostId) : IDomainEvent;
