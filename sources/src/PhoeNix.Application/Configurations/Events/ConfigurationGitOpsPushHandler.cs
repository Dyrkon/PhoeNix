using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Git;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Events;

namespace PhoeNix.Application.Configurations.Events;

internal sealed class ConfigurationGitOpsPushHandler(
    IServiceScopeFactory scopeFactory,
    ICurrentUserAccessor currentUserAccessor,
    ILogger<ConfigurationGitOpsPushHandler> logger)
    : INotificationHandler<ConfigurationChangedDomainEvent>
{
    public async Task Handle(ConfigurationChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var settingsRepo = scope.ServiceProvider.GetRequiredService<IAppSettingsRepository>();
        var settings = await settingsRepo.GetFirstAsync(cancellationToken);

        if (settings is null || settings.GitSyncMode != GitSyncMode.PushOnly)
            return;

        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
        {
            logger.LogWarning("GitOps push skipped: could not resolve current user");
            return;
        }

        // Queue push in background to avoid blocking the request
        _ = Task.Run(async () =>
        {
            try
            {
                using var pushScope = scopeFactory.CreateScope();
                var orchestrator = pushScope.ServiceProvider.GetRequiredService<IGitOpsPushOrchestrator>();
                var result = await orchestrator.PushAsync(userIdResult.Value, CancellationToken.None);
                if (result.IsFailure)
                    logger.LogWarning("GitOps push failed after configuration change: {Error}", result.Error.Description);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GitOps push threw an exception after configuration change");
            }
        }, CancellationToken.None);
    }
}
