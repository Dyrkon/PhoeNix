using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Events;

public sealed record ConfigurationChangedDomainEvent(ConfigurationId ConfigurationId) : IDomainEvent;
