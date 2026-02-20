using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.ProvisioningSessions;

public record ProvisioningSessionId(Guid value) : StronglyTypedId(value);