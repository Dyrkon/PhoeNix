using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Flakes;

public record FlakeHomeId(Guid Value) : StronglyTypedId(Value);