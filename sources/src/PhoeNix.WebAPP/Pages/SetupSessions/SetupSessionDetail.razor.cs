using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Contracts.Setup;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.Components.SetupSessions;

namespace PhoeNix.WebAPP.Pages.SetupSessions;

public partial class SetupSessionDetail : ComponentBase, IDisposable
{
    [Inject] private ISetupApiClient SetupApiClient { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Parameter] public Guid SessionId { get; set; }

    private SetupSessionDetailResponse? _session;
    private bool _isLoading = true;
    private Timer? _refreshTimer;

    private bool _isActive => _session is not null &&
                              _session.Targets.Any(t => t.SetupStage is not (
                                  SetupStage.Finished or SetupStage.Failed or SetupStage.Cancelled));

    private bool _hasFailed => _session?.Targets.Any(t => t.SetupStage is SetupStage.Failed) ?? false;

    private bool _hasRerunnable => _session?.Targets
        .Any(t => t.SetupStage is SetupStage.Failed or SetupStage.Cancelled) ?? false;

    private int _doneMachines => _session?.Targets
        .Count(t => t.SetupStage is SetupStage.Finished or SetupStage.Cancelled) ?? 0;

    private int _failedMachines => _session?.Targets
        .Count(t => t.SetupStage is SetupStage.Failed) ?? 0;

    private double _progressPercent => _session?.Targets.Count > 0
        ? (_doneMachines + _failedMachines) * 100.0 / _session.Targets.Count
        : 0;

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;
        await RefreshAsync();
        _isLoading = false;

        StartRefreshTimer();
    }

    private void StartRefreshTimer()
    {
        _refreshTimer?.Dispose();
        _refreshTimer = new Timer(
            async _ =>
            {
                await RefreshAsync();
                await InvokeAsync(StateHasChanged);

                if (!_isActive)
                    StopRefreshTimer();
            },
            null,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(5));
    }

    private void StopRefreshTimer()
    {
        _refreshTimer?.Dispose();
        _refreshTimer = null;
    }

    private async Task RefreshAsync()
    {
        var result = await SetupApiClient.GetSessionDetailAsync(SessionId);

        if (result.IsFailure || result.Value is null)
        {
            _session = null;
            return;
        }

        _session = result.Value;
    }

    private async Task OpenCancelDialogAsync()
    {
        var runningCount = _session?.Targets
            .Count(t => t.SetupStage is not (
                SetupStage.Finished or SetupStage.Failed or SetupStage.Cancelled)) ?? 0;

        var parameters = new DialogParameters<CancelSessionDialog>
        {
            { d => d.RunningMachineCount, runningCount }
        };

        var dialog = await DialogService.ShowAsync<CancelSessionDialog>(
            "Cancel Setup Session",
            parameters,
            new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true });

        var result = await dialog.Result;

        if (result is null || result.Canceled)
            return;

        var cancelResult = await SetupApiClient.CancelSessionAsync(SessionId);

        if (cancelResult.IsFailure)
        {
            Snackbar.Add("Failed to cancel session.", Severity.Error);
            return;
        }

        Snackbar.Add("Session cancelled.", Severity.Success);
        await RefreshAsync();
    }

    private async Task OpenRerunSessionDialogAsync()
    {
        var rerunCount = _session?.Targets
            .Count(t => t.SetupStage is SetupStage.Failed or SetupStage.Cancelled) ?? 0;

        var parameters = new DialogParameters<RerunSessionDialog>
        {
            { d => d.RerunMachineCount, rerunCount }
        };

        var dialog = await DialogService.ShowAsync<RerunSessionDialog>(
            "Rerun Setup Session",
            parameters,
            new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true });

        var result = await dialog.Result;

        if (result is null || result.Canceled)
            return;

        var targets = _session?.Targets
            .Where(t => t.SetupStage is SetupStage.Failed or SetupStage.Cancelled)
            .ToList() ?? [];

        var succeeded = 0;
        foreach (var target in targets)
        {
            if (await RerunMachineAsync(target))
                succeeded++;
        }

        if (succeeded == 0)
            Snackbar.Add("Failed to rerun machines.", Severity.Error);
        else
            Snackbar.Add($"Rerun started for {succeeded} machine{(succeeded == 1 ? "" : "s")}.", Severity.Success);

        await RefreshAsync();
        StartRefreshTimer();
    }

    private async Task RerunSingleMachineAsync(SetupTargetResponse target)
    {
        var success = await RerunMachineAsync(target);

        if (!success)
        {
            Snackbar.Add("Failed to rerun machine.", Severity.Error);
            return;
        }

        Snackbar.Add("Rerun started.", Severity.Success);
        await RefreshAsync();
        StartRefreshTimer();
    }

    private async Task<bool> RerunMachineAsync(SetupTargetResponse target)
    {
        if (target.SelectedConfigurationId is null || target.SelectedSystemId is null)
            return false;

        var request = new StartMachineSetupRequest(
            target.SelectedConfigurationId.Value,
            target.SelectedSystemId.Value);

        var result = await SetupApiClient.StartMachineSetupAsync(SessionId, target.MachineId, request);
        return result.IsSuccess;
    }

    internal static Color StageColor(SetupStage stage)
    {
        return stage switch
        {
            SetupStage.Finished => Color.Success,
            SetupStage.Failed => Color.Error,
            SetupStage.Cancelled => Color.Default,
            _ => Color.Warning
        };
    }

    internal static string StageIcon(SetupStage stage)
    {
        return stage switch
        {
            SetupStage.Finished => Icons.Material.Filled.CheckCircle,
            SetupStage.Failed => Icons.Material.Filled.Error,
            SetupStage.Cancelled => Icons.Material.Filled.Cancel,
            SetupStage.WaitingForPxe => Icons.Material.Filled.Router,
            SetupStage.Bootstrapped => Icons.Material.Filled.SystemUpdate,
            SetupStage.Probed => Icons.Material.Filled.Search,
            SetupStage.Orchestrated => Icons.Material.Filled.BuildCircle,
            _ => Icons.Material.Filled.HourglassEmpty
        };
    }

    public void Dispose()
    {
        _refreshTimer?.Dispose();
    }
}