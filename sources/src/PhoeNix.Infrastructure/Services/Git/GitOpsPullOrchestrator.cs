using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhoeNix.Application.Abstractions.Git;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.Git;

public sealed class GitOpsPullOrchestrator(
    IGitSyncService gitSyncService,
    IGitOpsImportService importService,
    IServiceScopeFactory scopeFactory,
    ILogger<GitOpsPullOrchestrator> logger) : IGitOpsPullOrchestrator
{
    public async Task<Result> PullAsync(UserId ownerId, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var settingsRepo = scope.ServiceProvider.GetRequiredService<IAppSettingsRepository>();
        var settings = await settingsRepo.GetFirstAsync(ct);

        if (settings is null)
            return Result.Failure(new Error("AppSettings.NotFound", "Application settings have not been initialized."));

        if (settings.GitSyncMode != GitSyncMode.PullOnly)
            return Result.Failure(new Error("GitSync.NotPullMode", "Git sync is not configured for pull-only mode."));

        // Pull latest from remote
        var pullResult = await gitSyncService.PullAsync(ct);
        if (pullResult.IsFailure)
        {
            logger.LogError("GitOps pull failed: {Error}", pullResult.Error.Description);
            return pullResult;
        }

        var repoPathResult = gitSyncService.GetLocalRepoPath();
        if (repoPathResult.IsFailure)
            return Result.Failure(repoPathResult.Error);

        // Import all data from repo
        var importResult = await importService.ImportAllAsync(
            ownerId, repoPathResult.Value, settings.GitPullDeleteOrphans, ct);
        if (importResult.IsFailure)
        {
            logger.LogError("GitOps import failed: {Error}", importResult.Error.Description);
            return importResult;
        }

        logger.LogInformation("GitOps pull completed successfully");
        return Result.Success();
    }
}
