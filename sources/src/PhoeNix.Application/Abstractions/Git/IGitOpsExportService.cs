using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Git;

public interface IGitOpsExportService
{
    Task<Result> ExportAllAsync(UserId ownerId, string repoPath, bool includeNixFiles, CancellationToken ct);
}
