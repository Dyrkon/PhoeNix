using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Modules;

public record TestId(Guid Value) : StronglyTypedId(Value, "test");