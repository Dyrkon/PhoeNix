using PhoeNix.Application.Models;
using PhoeNix.Application.Models.SshIdentity;
using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Models.Authentication;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Authentication;

public interface ISshKeyProvider
{
    Task<Result<SshIdentityMaterial>> GetOrCreateAsync(ProvisioningSession session, CancellationToken ct);
    Task<Result> RevokeAsync(ProvisioningSession session, CancellationToken ct);
}