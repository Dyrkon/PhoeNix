using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Configurations;

public record ConfigurationId(Guid Value) : StronglyTypedId(Value, "configuration");