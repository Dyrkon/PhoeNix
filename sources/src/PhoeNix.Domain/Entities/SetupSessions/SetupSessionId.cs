using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.SetupSessions;

public record SetupSessionId(Guid Value) : StronglyTypedId(Value);