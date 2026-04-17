using PhoeNix.Application.Models.Setup;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.States;

public sealed class SetupSessionsState : IDisposable
{
    private static readonly TimeSpan NewSessionGraceWindow = TimeSpan.FromMinutes(10);

    private readonly ISetupApiClient _setupApiClient;
    private Timer? _pollingTimer;

    public SetupSessionsState(ISetupApiClient setupApiClient)
    {
        _setupApiClient = setupApiClient;
    }

    public SetupSessionListResponse? ActiveSession { get; private set; }

    public event Action? StateChanged;

    public void StartPolling()
    {
        _pollingTimer ??= new Timer(
            _ => { _ = PollAsync(); },
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(3));
    }

    public void StopPolling()
    {
        _pollingTimer?.Dispose();
        _pollingTimer = null;
    }

    private async Task PollAsync()
    {
        var result = await _setupApiClient.GetSessionsAsync(1, 1);

        if (result.IsFailure || result.Value is null)
            return;

        var first = result.Value.Items.FirstOrDefault();

        var isActive = first is not null && IsActiveSession(first);
        var next = isActive ? first : null;

        var changed = next?.SessionId != ActiveSession?.SessionId
            || next?.TargetsDone != ActiveSession?.TargetsDone
            || next?.TargetsFailed != ActiveSession?.TargetsFailed;

        ActiveSession = next;

        if (changed)
            StateChanged?.Invoke();

        if (!isActive)
            StopPolling();
    }

    private static bool IsActiveSession(SetupSessionListResponse session)
    {
        if (session.TargetsTotal > 0)
            return session.TargetsDone + session.TargetsFailed < session.TargetsTotal;

        return DateTime.UtcNow - session.StartTime < NewSessionGraceWindow;
    }

    public void Dispose()
    {
        _pollingTimer?.Dispose();
    }
}