using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhoeNix.Application.Abstractions.Git;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.Git;

public sealed class GitOpsPushOrchestrator(
    IGitSyncService gitSyncService,
    IGitOpsExportService exportService,
    IServiceScopeFactory scopeFactory,
    ILogger<GitOpsPushOrchestrator> logger) : IGitOpsPushOrchestrator
{
    public async Task<Result> PushAsync(UserId ownerId, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var settingsRepo = scope.ServiceProvider.GetRequiredService<IAppSettingsRepository>();
        var settings = await settingsRepo.GetFirstAsync(ct);

        if (settings is null)
            return Result.Failure(new Error("AppSettings.NotFound", "Application settings have not been initialized."));

        if (settings.GitSyncMode != GitSyncMode.PushOnly)
            return Result.Failure(new Error("GitSync.NotPushMode", "Git sync is not configured for push-only mode."));

        // Initialize repo if needed
        var initResult = await gitSyncService.InitializeRepositoryAsync(ct);
        if (initResult.IsFailure)
            return initResult;

        var repoPathResult = gitSyncService.GetLocalRepoPath();
        if (repoPathResult.IsFailure)
            return Result.Failure(repoPathResult.Error);

        var repoPath = repoPathResult.Value;

        // Export all data to the repo
        var exportResult = await exportService.ExportAllAsync(ownerId, repoPath, settings.GitPushNixFiles, ct);
        if (exportResult.IsFailure)
        {
            logger.LogError("GitOps export failed: {Error}", exportResult.Error.Description);
            return exportResult;
        }

        // Commit and push
        var pushResult = await gitSyncService.CommitAndPushAsync(
            $"PhoeNix sync: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}", ct);
        if (pushResult.IsFailure)
        {
            logger.LogError("GitOps push failed: {Error}", pushResult.Error.Description);
            return pushResult;
        }

        logger.LogInformation("GitOps push completed successfully");
        return Result.Success();
    }
}
