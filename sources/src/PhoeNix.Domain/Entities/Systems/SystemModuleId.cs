using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Systems;

public record SystemModuleId(Guid Value) : StronglyTypedId(Value);