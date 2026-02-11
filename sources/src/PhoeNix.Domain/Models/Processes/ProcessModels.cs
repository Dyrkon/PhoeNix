namespace PhoeNix.Domain.Models.Processes;

public record ProcessResult(
    int ReturnCode,
    string StandardOutput,
    string ErrorOutput,
    TimeSpan RunDuration,
    bool Canceled,
    bool TimedOut,
    DateTime StartTime);