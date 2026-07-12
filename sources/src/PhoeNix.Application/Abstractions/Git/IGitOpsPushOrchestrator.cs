using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Git;

public interface IGitOpsPushOrchestrator
{
    Task<Result> PushAsync(UserId ownerId, CancellationToken ct);
}
