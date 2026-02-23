using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Bootstrap;

public sealed record BootstrapBuildProgress(
    ProvisioningStage Stage,
    string? Detail = null);

