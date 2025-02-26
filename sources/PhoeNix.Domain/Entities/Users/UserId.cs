using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Users;

public record UserId(Guid Value) : StronglyTypedId(Value);