using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.VmHosts;

public record VmHostId(Guid Value) : StronglyTypedId(Value);
