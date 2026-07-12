using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhoeNix.Application.Abstractions.Git;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.AppSettings;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.Git;

public sealed class GitSyncService(
    IProcessRunner processRunner,
    IServiceScopeFactory scopeFactory,
    ILogger<GitSyncService> logger) : IGitSyncService
{
    private const string GitExecutable = "git";

    public async Task<Result> InitializeRepositoryAsync(CancellationToken ct)
    {
        var settingsResult = await GetSettingsAsync(ct);
        if (settingsResult.IsFailure)
            return Result.Failure(settingsResult.Error);

        var settings = settingsResult.Value;
        var repoPath = BuildRepoPath(settings);

        if (Directory.Exists(Path.Combine(repoPath, ".git")))
        {
            // Update remote URL in case settings changed
            RunGit(["remote", "set-url", "origin", GetEffectiveRemoteUrl(settings)], repoPath, settings, ct);
            return Result.Success();
        }

        Directory.CreateDirectory(repoPath);

        var initResult = RunGit(["init", "--initial-branch", settings.GitBranch], repoPath, settings, ct);
        if (initResult.IsFailure)
            return Result.Failure(initResult.Error);

        var remoteResult = RunGit(["remote", "add", "origin", GetEffectiveRemoteUrl(settings)], repoPath, settings, ct);
        if (remoteResult.IsFailure)
            return Result.Failure(remoteResult.Error);

        logger.LogInformation("Initialized git repository at {Path} with remote {Remote}",
            repoPath, settings.GitRemoteUrl);

        return Result.Success();
    }

    public async Task<Result> PullAsync(CancellationToken ct)
    {
        var settingsResult = await GetSettingsAsync(ct);
        if (settingsResult.IsFailure)
            return Result.Failure(settingsResult.Error);

        var settings = settingsResult.Value;
        var repoPath = BuildRepoPath(settings);

        var ensureResult = await EnsureRepositoryAsync(settings, repoPath, ct);
        if (ensureResult.IsFailure)
            return ensureResult;

        var fetchResult = RunGit(["fetch", "origin", settings.GitBranch], repoPath, settings, ct);
        if (fetchResult.IsFailure)
            return Result.Failure(fetchResult.Error);

        var resetResult = RunGit(["reset", "--hard", $"origin/{settings.GitBranch}"], repoPath, settings, ct);
        if (resetResult.IsFailure)
            return Result.Failure(resetResult.Error);

        logger.LogInformation("Pulled latest from origin/{Branch}", settings.GitBranch);
        return Result.Success();
    }

    public async Task<Result> CommitAndPushAsync(string message, CancellationToken ct)
    {
        var settingsResult = await GetSettingsAsync(ct);
        if (settingsResult.IsFailure)
            return Result.Failure(settingsResult.Error);

        var settings = settingsResult.Value;
        var repoPath = BuildRepoPath(settings);

        var ensureResult = await EnsureRepositoryAsync(settings, repoPath, ct);
        if (ensureResult.IsFailure)
            return ensureResult;

        var addResult = RunGit(["add", "-A"], repoPath, settings, ct);
        if (addResult.IsFailure)
            return Result.Failure(addResult.Error);

        // Check if there are staged changes
        var statusResult = RunGit(["status", "--porcelain"], repoPath, settings, ct);
        if (statusResult.IsFailure)
            return Result.Failure(statusResult.Error);

        if (string.IsNullOrWhiteSpace(statusResult.Value.StandardOutput))
        {
            logger.LogDebug("No changes to commit");
            return Result.Success();
        }

        var commitResult = RunGit(
            ["commit", "-m", message, "--author", "PhoeNix <phoenix@localhost>"],
            repoPath, settings, ct);
        if (commitResult.IsFailure)
            return Result.Failure(commitResult.Error);

        var pushResult = RunGit(["push", "origin", settings.GitBranch], repoPath, settings, ct);
        if (pushResult.IsFailure)
            return Result.Failure(pushResult.Error);

        logger.LogInformation("Committed and pushed to origin/{Branch}: {Message}", settings.GitBranch, message);
        return Result.Success();
    }

    public async Task<Result<bool>> HasRemoteChangesAsync(CancellationToken ct)
    {
        var settingsResult = await GetSettingsAsync(ct);
        if (settingsResult.IsFailure)
            return Result.Failure<bool>(settingsResult.Error);

        var settings = settingsResult.Value;
        var repoPath = BuildRepoPath(settings);

        var ensureResult = await EnsureRepositoryAsync(settings, repoPath, ct);
        if (ensureResult.IsFailure)
            return Result.Failure<bool>(ensureResult.Error);

        var fetchResult = RunGit(["fetch", "origin", settings.GitBranch], repoPath, settings, ct);
        if (fetchResult.IsFailure)
            return Result.Failure<bool>(fetchResult.Error);

        var diffResult = RunGit(
            ["rev-list", "--count", $"HEAD..origin/{settings.GitBranch}"],
            repoPath, settings, ct);
        if (diffResult.IsFailure)
            return Result.Failure<bool>(diffResult.Error);

        var count = int.TryParse(diffResult.Value.StandardOutput.Trim(), out var c) ? c : 0;
        return Result.Success(count > 0);
    }

    public Result<string> GetLocalRepoPath()
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IAppSettingsRepository>();
        var settings = repo.GetFirstAsync(CancellationToken.None).GetAwaiter().GetResult();

        if (settings is null)
            return Result.Failure<string>(new Error("AppSettings.NotFound", "Application settings have not been initialized."));

        return Result.Success(BuildRepoPath(settings));
    }

    private async Task<Result<AppSettings>> GetSettingsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IAppSettingsRepository>();
        var settings = await repo.GetFirstAsync(ct);

        if (settings is null)
            return Result.Failure<AppSettings>(new Error("AppSettings.NotFound", "Application settings have not been initialized."));

        if (settings.GitSyncMode == GitSyncMode.None)
            return Result.Failure<AppSettings>(new Error("GitSync.Disabled", "Git sync is not enabled."));

        if (string.IsNullOrWhiteSpace(settings.GitRemoteUrl))
            return Result.Failure<AppSettings>(new Error("GitSync.NoRemote", "Git remote URL is not configured."));

        return Result.Success(settings);
    }

    private async Task<Result> EnsureRepositoryAsync(AppSettings settings, string repoPath, CancellationToken ct)
    {
        if (!Directory.Exists(Path.Combine(repoPath, ".git")))
        {
            var initResult = await InitializeRepositoryAsync(ct);
            if (initResult.IsFailure)
                return initResult;
        }

        return Result.Success();
    }

    private static string BuildRepoPath(AppSettings settings)
    {
        return Path.Combine(settings.FileStorageRootPath, "git-sync");
    }

    private static string GetEffectiveRemoteUrl(AppSettings settings)
    {
        if (settings.GitAuthMethod == GitAuthMethod.Token
            && !string.IsNullOrWhiteSpace(settings.GitAuthSecret)
            && settings.GitRemoteUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return settings.GitRemoteUrl.Replace("https://", $"https://x-access-token:{settings.GitAuthSecret}@");
        }

        return settings.GitRemoteUrl;
    }

    private Result<Application.Models.Processes.ProcessResult> RunGit(
        List<string> arguments,
        string workingDirectory,
        AppSettings settings,
        CancellationToken ct)
    {
        var env = BuildEnvironment(settings);
        return processRunner.RunProcess(GitExecutable, arguments, ct,
            environmentVariables: env,
            workingDirectory: workingDirectory,
            timeOut: TimeSpan.FromMinutes(5));
    }

    private static Dictionary<string, string>? BuildEnvironment(AppSettings settings)
    {
        if (settings.GitAuthMethod == GitAuthMethod.SshKey && !string.IsNullOrWhiteSpace(settings.GitAuthSecret))
        {
            return new Dictionary<string, string>
            {
                ["GIT_SSH_COMMAND"] = $"ssh -i {settings.GitAuthSecret} -o StrictHostKeyChecking=no"
            };
        }

        if (settings.GitAuthMethod == GitAuthMethod.Token)
        {
            return new Dictionary<string, string>
            {
                ["GIT_TERMINAL_PROMPT"] = "0"
            };
        }

        return null;
    }
}
