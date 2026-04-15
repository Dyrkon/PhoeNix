using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.Outbox;
using PhoeNix.Application.Options;

namespace PhoeNix.Persistence.Outbox;

internal sealed class OutboxProcessorBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessorBackgroundService> logger)
    : BackgroundService
{
    private readonly OutboxOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.PollInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unexpected error while processing the outbox.");
            }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        List<Guid> messageIds;

        using (var scope = serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var nowUtc = DateTime.UtcNow;

            messageIds = await dbContext.OutboxMessages
                .Where(x => x.ProcessedOnUtc == null && (x.NextAttemptOnUtc == null || x.NextAttemptOnUtc <= nowUtc))
                .OrderBy(x => x.OccurredOnUtc)
                .Select(x => x.Id)
                .Take(_options.BatchSize)
                .ToListAsync(cancellationToken);
        }

        foreach (var messageId in messageIds) await ProcessMessageAsync(messageId, cancellationToken);
    }

    private async Task ProcessMessageAsync(Guid messageId, CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        var serializer = scope.ServiceProvider.GetRequiredService<IOutboxMessageSerializer>();

        var message = await dbContext.OutboxMessages.FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);
        if (message is null || message.ProcessedOnUtc is not null)
            return;

        try
        {
            var domainEvent = serializer.Deserialize(message.Type, message.Content);

            await publisher.Publish(domainEvent, cancellationToken);

            message.MarkProcessed(DateTime.UtcNow);
        }
        catch (Exception exception)
        {
            message.MarkFailed(
                DateTime.UtcNow,
                exception.ToString(),
                CalculateNextAttemptUtc(DateTime.UtcNow, message.RetryCount + 1));

            logger.LogError(exception, "Failed to process outbox message {OutboxMessageId}.", message.Id);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static DateTime CalculateNextAttemptUtc(DateTime nowUtc, int retryCount)
    {
        var delaySeconds = Math.Min(100, (int)Math.Pow(2, Math.Min(retryCount, 8)));
        return nowUtc.AddSeconds(delaySeconds);
    }
}