using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Configurations;

public record ConfigurationSystemId(Guid Value) : StronglyTypedId(Value);