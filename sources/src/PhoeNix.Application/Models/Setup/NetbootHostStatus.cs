namespace PhoeNix.Application.Models.Setup;

public sealed record NetbootHostStatus(
    bool Running,
    int? ProcessId,
    DateTime? StartedAtUtc,
    string? Detail = null);