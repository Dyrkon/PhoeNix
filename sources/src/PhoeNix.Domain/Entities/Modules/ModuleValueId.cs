using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Modules;

public record ModuleValueId(Guid Value) : StronglyTypedId(Value);