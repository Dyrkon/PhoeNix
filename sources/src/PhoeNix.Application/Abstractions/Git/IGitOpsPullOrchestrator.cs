using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Git;

public interface IGitOpsPullOrchestrator
{
    Task<Result> PullAsync(UserId ownerId, CancellationToken ct);
}
