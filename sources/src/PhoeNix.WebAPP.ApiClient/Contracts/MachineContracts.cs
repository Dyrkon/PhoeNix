namespace PhoeNix.WebAPP.ApiClient.Contracts;

public sealed record CreateMachineRequest(
    string? Title,
    bool Enabled,
    string? MacAddress,
    Architecture Architecture,
    InstallDiskSelectionPreference InstallDiskSelectionPreference);

public sealed record MachineListResponse(
    Guid Id,
    string Title,
    bool Enabled,
    string MacAddress,
    Architecture Architecture,
    MachineState MachineState);