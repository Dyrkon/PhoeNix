using PhoeNix.Domain.Enums;

namespace PhoeNix.Contracts.Setup;

public sealed record StartMachineSetupRequest(Guid ConfigurationId, Guid SystemId);

public sealed record BootstrapCallbackRequest(Guid SessionId, Guid MachineId);

public sealed record SetupErrorSnapshotResponse(
    string? Code,
    string? Description,
    string? Source,
    DateTime OccurredAtUtc);

public sealed record SetupStatusResponse(
    SetupStage Stage,
    DateTime? LastTransitionAtUtc,
    SetupErrorSnapshotResponse? LastError);

public sealed record SetupSessionListResponse(
    Guid SessionId,
    DateTime StartTime,
    DateTime? LastTransitionTime,
    int TargetsTotal,
    int TargetsDone,
    int TargetsFailed);

public sealed record SetupSessionDetailResponse(
    Guid SessionId,
    DateTime StartTime,
    DateTime? LastTransitionTime,
    DateTime? CredentialsExpireAt,
    IReadOnlyList<SetupTargetResponse> Targets);

public sealed record SetupTargetResponse(
    Guid MachineId,
    SetupStage SetupStage,
    DateTime? LastTransitionTime,
    string? LastErrorCode,
    string? LastErrorDescription,
    string? LastErrorSource,
    DateTime? LastErrorAt,
    string? IpAddress,
    Guid? SelectedSystemId,
    string? SelectedSystem,
    Guid? SelectedConfigurationId,
    string? SelectedConfiguration,
    IReadOnlyList<RankedDiskAssignmentResponse> DiskAssignments);

public sealed record RankedDiskAssignmentResponse(int Index, string DiskIdPath);
