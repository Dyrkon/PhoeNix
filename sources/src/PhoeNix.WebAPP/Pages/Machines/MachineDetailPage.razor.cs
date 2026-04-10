using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Application.Models.Machines;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.Components.Machines;
using PhoeNix.WebAPP.States;

namespace PhoeNix.WebAPP.Pages.Machines;

public partial class MachineDetailPage : ComponentBase, IDisposable
{
    [Inject] private IMachinesApiClient MachinesApiClient { get; set; } = null!;
    [Inject] private IDeploymentApiClient DeploymentApiClient { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [CascadingParameter] public MachineState MachineState { get; set; } = null!;

    [Parameter] public Guid MachineId { get; set; }

    private MachineDetailResponse? _machine;
    private bool _isLoading = true;

    private UpdateStatus CurrentStatus => MachineState.GetUpdate(MachineId)?.Status ?? UpdateStatus.None;

    private bool CanUpdate => _machine is { Enabled: true } && _machine.DeploymentSnapshot is not null;

    private string _canUpdateTooltip => _machine is null || _machine.DeploymentSnapshot is null
        ? "Machine must have a deployment snapshot to update"
        : !_machine.Enabled
            ? "Machine is disabled"
            : string.Empty;

    private string _updateStatusIcon => CurrentStatus switch
    {
        UpdateStatus.Success => Icons.Material.Filled.CheckCircle,
        UpdateStatus.Failed => Icons.Material.Filled.Error,
        _ => Icons.Material.Filled.Sync
    };

    private Color _updateStatusColor => CurrentStatus switch
    {
        UpdateStatus.Success => Color.Success,
        UpdateStatus.Failed => Color.Error,
        _ => Color.Default
    };

    private string _updateStatusTooltip => CurrentStatus switch
    {
        UpdateStatus.Success => "Last update succeeded — click for details",
        UpdateStatus.Failed => "Last update failed — click for details",
        _ => string.Empty
    };

    protected override void OnInitialized()
    {
        MachineState.StateChanged += OnMachineStateChanged;
    }

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;

        var response = await MachinesApiClient.GetMachineAsync(MachineId);

        if (response.IsFailure || response.Value is null)
        {
            Snackbar.Add("Failed to load machine detail.", Severity.Error);
            _machine = null;
            _isLoading = false;
            return;
        }

        _machine = response.Value;
        _isLoading = false;
    }

    private async Task UpdateConfigurationAsync()
    {
        if (_machine?.DeploymentSnapshot is null)
            return;

        var snapshot = _machine.DeploymentSnapshot;
        MachineState.SetUpdate(MachineId, new MachineUpdateEntry(UpdateStatus.InProgress,
            ConfigurationTitle: snapshot.ConfigurationTitle,
            SystemName: snapshot.SystemName));

        var result = await DeploymentApiClient.UpdateMachineAsync(
            snapshot.ConfigurationId,
            snapshot.SystemId,
            MachineId);

        if (result.IsSuccess)
        {
            MachineState.SetUpdate(MachineId, new MachineUpdateEntry(UpdateStatus.Success,
                ConfigurationTitle: snapshot.ConfigurationTitle,
                SystemName: snapshot.SystemName));

            var refreshed = await MachinesApiClient.GetMachineAsync(MachineId);
            if (refreshed.IsSuccess && refreshed.Value is not null)
                _machine = refreshed.Value;
        }
        else
        {
            MachineState.SetUpdate(MachineId, new MachineUpdateEntry(UpdateStatus.Failed,
                Error: result.Error,
                ConfigurationTitle: snapshot.ConfigurationTitle,
                SystemName: snapshot.SystemName));
        }

        StateHasChanged();
    }

    private async Task OpenUpdateResultDialogAsync()
    {
        var entry = MachineState.GetUpdate(MachineId);
        if (entry is null)
            return;

        var parameters = new DialogParameters<UpdateConfigurationResultDialog>
        {
            { x => x.IsSuccess, entry.Status == UpdateStatus.Success },
            { x => x.ErrorCode, entry.Error?.Code },
            { x => x.ErrorMessage, entry.Error?.Description },
            { x => x.ConfigurationTitle, entry.ConfigurationTitle },
            { x => x.SystemName, entry.SystemName }
        };

        await DialogService.ShowAsync<UpdateConfigurationResultDialog>("Update Result", parameters);
    }

    private void OnMachineStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        MachineState.StateChanged -= OnMachineStateChanged;
    }
}
