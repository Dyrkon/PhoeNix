using PhoeNix.WebAPP.ApiClient.Contracts;

namespace PhoeNix.WebAPP.States;

public enum UpdateStatus { None, InProgress, Success, Failed }

public sealed record MachineUpdateEntry(
    UpdateStatus Status,
    ApiError? Error = null,
    string? ConfigurationTitle = null,
    string? SystemName = null);

public class MachineState
{
    private readonly Dictionary<Guid, MachineUpdateEntry> _updates = new();

    public event Action? StateChanged;

    public void SetUpdate(Guid machineId, MachineUpdateEntry entry)
    {
        _updates[machineId] = entry;
        StateChanged?.Invoke();
    }

    public MachineUpdateEntry? GetUpdate(Guid machineId) =>
        _updates.GetValueOrDefault(machineId);
}
