using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Flakes;

public record FlakeModuleId(Guid Value) : StronglyTypedId(Value);