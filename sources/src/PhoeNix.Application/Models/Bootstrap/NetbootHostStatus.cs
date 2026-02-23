namespace PhoeNix.Application.Models.Bootstrap;

public sealed record NetbootHostStatus(
    bool Running,
    int? ProcessId,
    DateTime? StartedAtUtc,
    string? Detail = null);

