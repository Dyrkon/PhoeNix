using PhoeNix.Application.Models;
using PhoeNix.Application.Models.SshIdentity;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Authentication;

public interface ISetupSshKeyProvider
{
    Task<Result<SshIdentityMaterial>> GetOrCreateAsync(SetupSession session, CancellationToken cancellationToken);
    Task<Result> RevokeAsync(SetupSession session, CancellationToken cancellationToken);
}