using PhoeNix.Application.Models.SshIdentity;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Bootstrap;

public sealed record BootstrapBuildRequest(
    ProvisioningSessionId SessionId,
    MachineId MachineId,
    Architecture Architecture,
    SshIdentityMaterial SshIdentity,
    string CallbackToken,
    string? UserAuthorizedKey,
    string? AdditionalKernelParams);

