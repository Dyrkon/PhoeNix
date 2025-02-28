using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Flakes;

public record FlakeSystemId(Guid Value) : StronglyTypedId(Value);