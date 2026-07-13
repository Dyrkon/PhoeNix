using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Contracts.VmHosts;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.Pages.VmHosts;

public partial class VmHostDetailPage : ComponentBase
{
    [Inject] private IVmHostsApiClient VmHostsApiClient { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Parameter] public Guid VmHostId { get; set; }

    private VmHostDetailResponse? _vmHost;
    private bool _isLoading = true;
    private bool _isSyncing;
    private bool _isTesting;
    private bool _isSaving;
    private bool _isDiscovering;
    private bool _showSecret;

    private string _editName = string.Empty;
    private string _editHost = string.Empty;
    private int? _editPort;
    private string? _editUsername;
    private string? _editSecret;
    private string? _editExtraConfig;

    private List<DiscoveredVmResponse> _discoveredVms = [];

    protected override async Task OnParametersSetAsync()
    {
        await LoadVmHostAsync();
    }

    private async Task LoadVmHostAsync()
    {
        _isLoading = true;
        var result = await VmHostsApiClient.GetVmHostAsync(VmHostId);

        if (result is { IsSuccess: true, Value: not null })
        {
            _vmHost = result.Value;
            PopulateEditFields();
        }
        else
        {
            Snackbar.Add("Failed to load VM host.", Severity.Error);
            _vmHost = null;
        }

        _isLoading = false;
    }

    private void PopulateEditFields()
    {
        if (_vmHost is null) return;
        _editName = _vmHost.Name;
        _editHost = _vmHost.Host;
        _editPort = _vmHost.Port;
        _editUsername = _vmHost.Username;
        _editSecret = null;
        _editExtraConfig = _vmHost.ExtraConfig;
    }

    private async Task SaveCredentialsAsync()
    {
        _isSaving = true;
        var request = new UpdateVmHostRequest(
            _editName, _editHost, _editPort, _editUsername, _editSecret, _editExtraConfig);

        var result = await VmHostsApiClient.UpdateVmHostAsync(VmHostId, request);
        _isSaving = false;

        if (result.IsFailure)
        {
            Snackbar.Add($"Save failed: {result.Error?.Description}", Severity.Error);
            return;
        }

        Snackbar.Add("Changes saved.", Severity.Success);
        await LoadVmHostAsync();
    }

    private async Task SyncResourcesAsync()
    {
        _isSyncing = true;
        var result = await VmHostsApiClient.SyncResourcesAsync(VmHostId);
        _isSyncing = false;

        Snackbar.Add(
            result.IsSuccess ? "Resources synced." : $"Sync failed: {result.Error?.Description}",
            result.IsSuccess ? Severity.Success : Severity.Error);

        if (result.IsSuccess)
            await LoadVmHostAsync();
    }

    private async Task TestConnectionAsync()
    {
        _isTesting = true;
        var result = await VmHostsApiClient.TestConnectionAsync(VmHostId);
        _isTesting = false;

        Snackbar.Add(
            result.IsSuccess ? "Connection successful." : $"Connection failed: {result.Error?.Description}",
            result.IsSuccess ? Severity.Success : Severity.Error);
    }

    private async Task DiscoverVmsAsync()
    {
        _isDiscovering = true;
        var result = await VmHostsApiClient.DiscoverVmsAsync(VmHostId);
        _isDiscovering = false;

        if (result is { IsSuccess: true, Value: not null })
            _discoveredVms = result.Value.ToList();
        else
            Snackbar.Add($"Discovery failed: {result.Error?.Description}", Severity.Error);
    }

    private async Task OpenCreateVmDialogAsync()
    {
        var parameters = new DialogParameters<CreateVmDialog>
        {
            { x => x.VmHostId, VmHostId },
            { x => x.VmHostName, _vmHost?.Name ?? "" }
        };

        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<CreateVmDialog>("Create VM", parameters, options);
        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            await LoadVmHostAsync();
            await DiscoverVmsAsync();
        }
    }

    private static double ResourcePercent(long used, long total)
        => total > 0 ? (double)used / total * 100 : 0;

    private static string FormatMb(long mb) => mb >= 1024 ? $"{mb / 1024.0:F1} GB" : $"{mb} MB";

    private static Color PowerStateColor(VmPowerState state) => state switch
    {
        VmPowerState.Running => Color.Success,
        VmPowerState.Stopped => Color.Default,
        VmPowerState.Paused => Color.Warning,
        _ => Color.Error
    };
}
