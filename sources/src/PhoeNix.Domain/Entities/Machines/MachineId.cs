using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Machines;

public record MachineId(Guid Value) : StronglyTypedId(Value);