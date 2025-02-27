using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Modules;

public record ModuleEntryId(Guid Value) : StronglyTypedId(Value);