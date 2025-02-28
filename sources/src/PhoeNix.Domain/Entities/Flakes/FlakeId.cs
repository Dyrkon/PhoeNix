using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Flakes;

public record FlakeId(Guid Value) : StronglyTypedId(Value);