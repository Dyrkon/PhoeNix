using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Inputs;

public record InputId(Guid Value) : StronglyTypedId(Value);