using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhoeNix.Application.Abstractions.Git;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Infrastructure.Services.Git;

public sealed class GitOpsPollBackgroundService(
    IServiceScopeFactory scopeFactory,
    IGitSyncService gitSyncService,
    ILogger<GitOpsPollBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait a bit before starting to let the app initialize
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var intervalMinutes = await GetPollingIntervalAsync(stoppingToken);

            if (intervalMinutes is null or <= 0)
            {
                // Polling disabled, check again in 5 minutes
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                continue;
            }

            try
            {
                var hasChanges = await gitSyncService.HasRemoteChangesAsync(stoppingToken);
                if (hasChanges.IsSuccess && hasChanges.Value)
                {
                    logger.LogInformation("GitOps poll detected remote changes, triggering pull");

                    using var scope = scopeFactory.CreateScope();
                    var orchestrator = scope.ServiceProvider.GetRequiredService<IGitOpsPullOrchestrator>();
                    var settingsRepo = scope.ServiceProvider.GetRequiredService<IAppSettingsRepository>();
                    var settings = await settingsRepo.GetFirstAsync(stoppingToken);

                    if (settings is not null)
                    {
                        var result = await orchestrator.PullAsync(settings.OwnerId, stoppingToken);
                        if (result.IsFailure)
                            logger.LogWarning("GitOps poll pull failed: {Error}", result.Error.Description);
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "GitOps poll encountered an error");
            }

            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes.Value), stoppingToken);
        }
    }

    private async Task<int?> GetPollingIntervalAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var settingsRepo = scope.ServiceProvider.GetRequiredService<IAppSettingsRepository>();
            var settings = await settingsRepo.GetFirstAsync(ct);

            if (settings is null || settings.GitSyncMode != GitSyncMode.PullOnly)
                return null;

            return settings.GitPullPollingIntervalMinutes;
        }
        catch
        {
            return null;
        }
    }
}
