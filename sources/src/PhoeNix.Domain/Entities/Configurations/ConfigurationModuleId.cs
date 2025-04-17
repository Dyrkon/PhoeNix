using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Configurations;

public record ConfigurationModuleId(Guid Value) : StronglyTypedId(Value);