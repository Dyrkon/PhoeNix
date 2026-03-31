namespace PhoeNix.WebAPP.ApiClient.Contracts;

public sealed record SetupErrorSnapshotResponse(
    string? Code,
    string? Description,
    string? Source,
    DateTime OccurredAtUtc);

public sealed record SetupStatusResponse(
    SetupStage Stage,
    DateTime? LastTransitionAtUtc,
    SetupErrorSnapshotResponse? LastError);

public enum Architecture
{
    X86Linux = 0,
    Aarch64Linux = 1,
    X86Darwin = 2,
    Aarch64Darwin = 3
}

public enum ModuleType
{
    Generic = 0,
    System = 1
}

public enum EntryBindingKind
{
    UserProvided = 0,
    RankedDiskCandidate = 1
}

public enum EntryValueKind
{
    Text = 1,
    IntegerRange = 2,
    DecimalRange = 3,
    SingleChoice = 4
}

public enum InstallDiskSelectionPreference
{
    Biggest = 0,
    Fastest = 1,
    FastestAndBiggest = 2,
    BiggestAndFastest = 3
}

public enum SetupStage
{
    Created = 0,
    WaitingForPxe = 1,
    ArtefactsAssigned = 2,
    Bootstrapped = 3,
    Probed = 4,
    Orchestrated = 5,
    Finished = 6,
    Failed = 7,
    Cancelled = 8
}

public enum UserInputType
{
    Text = 0,
    Range = 1,
    MultiChoice = 2
}