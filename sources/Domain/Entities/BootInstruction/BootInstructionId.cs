using Domain.Primitives;

namespace Domain.Entities.BootInstruction;

public record BootInstructionId(Guid Value) : StronglyTypedId(Value);