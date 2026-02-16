using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Modules;

public record ModuleTemplateId(Guid Value) : StronglyTypedId(Value, "module");