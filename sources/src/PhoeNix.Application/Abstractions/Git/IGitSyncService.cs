using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Git;

public interface IGitSyncService
{
    Task<Result> InitializeRepositoryAsync(CancellationToken ct);
    Task<Result> PullAsync(CancellationToken ct);
    Task<Result> CommitAndPushAsync(string message, CancellationToken ct);
    Task<Result<bool>> HasRemoteChangesAsync(CancellationToken ct);
    Result<string> GetLocalRepoPath();
}
