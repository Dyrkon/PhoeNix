using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Modules;

public record EntryValueId(Guid Value) : StronglyTypedId(Value);