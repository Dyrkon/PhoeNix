using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.SystemUsers;

public record SystemUserId(Guid Value) : StronglyTypedId(Value);