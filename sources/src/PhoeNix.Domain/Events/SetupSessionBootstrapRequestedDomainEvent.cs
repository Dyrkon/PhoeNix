using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Events;

public sealed record SetupSessionBootstrapRequestedDomainEvent(
    SetupSessionId SessionId) : IDomainEvent;
