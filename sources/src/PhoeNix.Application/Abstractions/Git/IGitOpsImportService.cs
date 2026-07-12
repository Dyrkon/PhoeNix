using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Git;

public interface IGitOpsImportService
{
    Task<Result> ImportAllAsync(UserId ownerId, string repoPath, bool deleteOrphans, CancellationToken ct);
}
