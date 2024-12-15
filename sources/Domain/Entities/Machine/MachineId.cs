using Domain.Primitives;

namespace Domain.Entities.Machine;

public record MachineId(Guid Value) : StronglyTypedId(Value);