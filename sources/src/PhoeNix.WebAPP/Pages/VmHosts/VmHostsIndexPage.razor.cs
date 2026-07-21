using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Contracts.VmHosts;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.Extensions;

namespace PhoeNix.WebAPP.Pages.VmHosts;

public partial class VmHostsIndexPage : ComponentBase
{
    [Inject] private IVmHostsApiClient VmHostsApiClient { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private List<VmHostListResponse> _vmHosts = [];
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadVmHostsAsync();
    }

    private async Task LoadVmHostsAsync()
    {
        _isLoading = true;
        var result = await VmHostsApiClient.ListVmHostsAsync();

        if (result.IsFailure || result.Value is null)
        {
            Snackbar.Add("Failed to load VM hosts.", Severity.Error);
            _vmHosts = [];
        }
        else
        {
            _vmHosts = result.Value.ToList();
        }

        _isLoading = false;
    }

    private void NavigateToDetail(Guid vmHostId)
    {
        NavigationManager.NavigateToVmHostDetail(vmHostId);
    }

    private async Task OpenRegisterDialogAsync()
    {
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<RegisterVmHostDialog>("Register VM Host", options);
        var result = await dialog.Result;

        if (result is { Canceled: false })
            await LoadVmHostsAsync();
    }

    private async Task SyncResourcesAsync(Guid vmHostId)
    {
        var result = await VmHostsApiClient.SyncResourcesAsync(vmHostId);
        Snackbar.Add(
            result.IsSuccess ? "Resources synced." : $"Sync failed: {result.Error?.Description}",
            result.IsSuccess ? Severity.Success : Severity.Error);

        if (result.IsSuccess)
            await LoadVmHostsAsync();
    }

    private async Task TestConnectionAsync(Guid vmHostId)
    {
        var result = await VmHostsApiClient.TestConnectionAsync(vmHostId);
        Snackbar.Add(
            result.IsSuccess ? "Connection successful." : $"Connection failed: {result.Error?.Description}",
            result.IsSuccess ? Severity.Success : Severity.Error);
    }

    private async Task DeleteVmHostAsync(VmHostListResponse vmHost)
    {
        var result = await VmHostsApiClient.RemoveVmHostAsync(vmHost.Id);
        Snackbar.Add(
            result.IsSuccess ? "VM host removed." : $"Failed: {result.Error?.Description}",
            result.IsSuccess ? Severity.Success : Severity.Error);

        if (result.IsSuccess)
            await LoadVmHostsAsync();
    }

    private static double ResourcePercent(long used, long total)
        => total > 0 ? (double)used / total * 100 : 0;

    private static string FormatMb(long mb) => mb >= 1024 ? $"{mb / 1024.0:F1} GB" : $"{mb} MB";
}
