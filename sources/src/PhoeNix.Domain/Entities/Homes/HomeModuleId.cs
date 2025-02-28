using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Homes;

public record HomeModuleId(Guid Value) : StronglyTypedId(Value);