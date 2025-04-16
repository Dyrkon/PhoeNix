using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Configurations;

public record ConfigurationHomeId(Guid Value) : StronglyTypedId(Value);