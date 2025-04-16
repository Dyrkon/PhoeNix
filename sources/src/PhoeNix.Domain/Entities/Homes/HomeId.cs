using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Homes;

public record HomeId(Guid Value) : StronglyTypedId(Value);