namespace PhoeNix.Application.Models.HardwareProbing;

public sealed record HardwareProbeResult(
    string RawReport,
    DateTime ObservedAtUtc);