using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhoeNix.Application.Abstractions.Bootstrap;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Infrastructure.Services.Setup;

internal sealed class BootstrapBackgroundService(
    IBootstrapSessionQueue queue,
    INetbootHostService netbootHostService,
    IServiceScopeFactory scopeFactory,
    ILogger<BootstrapBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var sessionId in queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await BuildAsync(sessionId, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception building bootstrap image for session {SessionId}.", sessionId.Value);
            }
        }
    }

    private async Task BuildAsync(
        Domain.Entities.SetupSessions.SetupSessionId sessionId,
        CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var bootstrapImageBuilder = scope.ServiceProvider.GetRequiredService<IBootstrapImageBuilder>();
        var setupSessionRepository = scope.ServiceProvider.GetRequiredService<ISetupSessionRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var session = await setupSessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            logger.LogError("Bootstrap background service: session {SessionId} not found.", sessionId.Value);
            return;
        }

        logger.LogInformation("Building bootstrap image for session {SessionId}.", sessionId.Value);

        // TODO need to add support for more architectures
        var imageResult = await bootstrapImageBuilder.BuildAsync(Architecture.X86Linux, cancellationToken);

        if (imageResult.IsFailure)
        {
            logger.LogError(
                "Bootstrap image build failed for session {SessionId}: {ErrorCode} — {ErrorMessage}",
                sessionId.Value,
                imageResult.Error.Code,
                imageResult.Error.Description);

            session.MarkBootstrapFailed(imageResult.Error.Description ?? imageResult.Error.Code);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var assignResult = session.AssignBootstrapArtefact(
            imageResult.Value.Kernel,
            imageResult.Value.RamDisk,
            imageResult.Value.Init);

        if (assignResult.IsFailure)
        {
            logger.LogError(
                "Failed to assign bootstrap artefact for session {SessionId}: {ErrorCode} — {ErrorMessage}",
                sessionId.Value,
                assignResult.Error.Code,
                assignResult.Error.Description);

            session.MarkBootstrapFailed(assignResult.Error.Description ?? assignResult.Error.Code);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var startHostResult = await netbootHostService.StartAsync(cancellationToken);
        if (startHostResult.IsFailure)
        {
            logger.LogWarning(
                "Failed to start netboot host after bootstrap build for session {SessionId}: {ErrorCode} — {ErrorMessage}",
                sessionId.Value,
                startHostResult.Error.Code,
                startHostResult.Error.Description);
        }

        logger.LogInformation("Bootstrap image ready for session {SessionId}.", sessionId.Value);
    }
}
