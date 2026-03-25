namespace PhoeNix.Application.Options;

public sealed class OutboxOptions
{
    public int BatchSize { get; set; } = 20;
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(2);
}