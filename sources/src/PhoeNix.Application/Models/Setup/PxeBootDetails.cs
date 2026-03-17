namespace PhoeNix.Application.Models.Setup;

public sealed record PxeBootDetails(
    string Kernel,
    IReadOnlyList<string> Initrd,
    string Cmdline,
    string? Message = null);