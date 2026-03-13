namespace PhoeNix.Application.Models.Bootstrap;

public sealed record PxeBootDetails(
    string Kernel,
    IReadOnlyList<string> Initrd,
    string Cmdline,
    string? Message = null);