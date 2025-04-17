using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Modules;

public record ModuleId(Guid Value) : StronglyTypedId(Value);