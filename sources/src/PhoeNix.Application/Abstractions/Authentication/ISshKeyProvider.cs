using PhoeNix.Application.Models;
using PhoeNix.Application.Models.SshIdentity;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Authentication;

public interface ISshKeyProvider
{
    Task<Result<SshIdentityMaterial>> GetOrCreateAsync(SetupSession session, CancellationToken ct);
    Task<Result> RevokeAsync(SetupSession session, CancellationToken ct);
}