using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Systems;

public record SystemId(Guid Value) : StronglyTypedId(Value, "system");