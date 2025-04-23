using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Configurations;

public record ConfigurationInputId(Guid Value) : StronglyTypedId(Value);