using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Setup;

public sealed record BootstrapBuildProgress(
    SetupStage Stage,
    string? Detail = null);