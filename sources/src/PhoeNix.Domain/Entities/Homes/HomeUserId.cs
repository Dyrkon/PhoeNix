using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Homes;

public record HomeUserId(Guid Value) : StronglyTypedId(Value);