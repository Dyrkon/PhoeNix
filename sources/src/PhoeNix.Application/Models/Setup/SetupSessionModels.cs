using PhoeNix.Common.Models;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Setup;

public sealed record StartMachineSetupRequest(
    Guid ConfigurationId,
    Guid SystemId);

public sealed record CallbackModuleParameters(
    string FinalizeUrl,
    string BearerToken);

public sealed record DeployAccessModuleParameters(
    string DeployUser,
    string DeployCaPublicKey);

public sealed record BuiltInModuleParameters(
    CallbackModuleParameters? Callback = null,
    DeployAccessModuleParameters? DeployAccess = null);

public sealed record SetupSessionsRequest(
    int Page = 1,
    int PageSize = 15,
    string? Search = null,
    SortDirection SortDirection = SortDirection.Descending);

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