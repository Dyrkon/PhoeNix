using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Modules;

public record ModuleTestId(Guid Value) : StronglyTypedId(Value);