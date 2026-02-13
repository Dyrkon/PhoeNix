using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Configurations;

public record ModuleValueId(Guid Value) : StronglyTypedId(Value);