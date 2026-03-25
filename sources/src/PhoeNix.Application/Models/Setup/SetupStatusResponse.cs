using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Setup;

public sealed record SetupErrorSnapshotResponse(
    string Code,
    string Description,
    string Source,
    DateTime OccurredAtUtc);

public sealed record SetupStatusResponse(
    SetupStage Stage,
    DateTime? LastTransitionAtUtc,
    SetupErrorSnapshotResponse? LastError);