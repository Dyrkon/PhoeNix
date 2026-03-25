namespace PhoeNix.Application.Models.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    public Guid Id { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? NextAttemptOnUtc { get; private set; }

    public static OutboxMessage Create(
        DateTime occurredOnUtc,
        string type,
        string content)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = occurredOnUtc,
            Type = type,
            Content = content
        };
    }

    public void MarkProcessed(DateTime processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        Error = null;
        NextAttemptOnUtc = null;
    }

    public void MarkFailed(DateTime nowUtc, string error, DateTime nextAttemptOnUtc)
    {
        RetryCount++;
        Error = error;
        NextAttemptOnUtc = nextAttemptOnUtc;
    }
}