using System.Net;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Systems;

namespace PhoeNix.Domain.Entities.Machines;

public sealed record ProvisioningSnapshot(
    ConfigurationId ConfigurationId,
    SystemId SystemId,
    IPAddress LastKnownIpAddress,
    DateTime ProvisionedAtUtc);