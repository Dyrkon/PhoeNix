using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Contracts.VmHosts;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.Pages.VmHosts;

public partial class CreateVmDialog : ComponentBase
{
    [Inject] private IVmHostsApiClient VmHostsApiClient { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public Guid VmHostId { get; set; }
    [Parameter] public string VmHostName { get; set; } = string.Empty;

    private string _name = string.Empty;
    private Architecture _architecture = Architecture.X86Linux;
    private int _cpuCores = 2;
    private int _memoryMb = 2048;
    private int _diskSizeGb = 32;
    private string? _networkBridge;
    private InstallDiskSelectionPreference _diskPref = InstallDiskSelectionPreference.Biggest;
    private bool _isCreating;
    private string? _errorMessage;

    private void Cancel() => MudDialog.Cancel();

    private async Task CreateAsync()
    {
        if (string.IsNullOrWhiteSpace(_name))
        {
            _errorMessage = "VM name is required.";
            return;
        }

        _isCreating = true;
        _errorMessage = null;

        var request = new CreateMachineVmRequest(
            VmHostId, _name, _cpuCores, _memoryMb, _diskSizeGb,
            _networkBridge, _architecture, true, _diskPref);

        var result = await VmHostsApiClient.CreateMachineVmAsync(request);

        _isCreating = false;

        if (result.IsFailure)
        {
            _errorMessage = result.Error?.Description ?? "VM creation failed.";
            return;
        }

        MudDialog.Close(DialogResult.Ok(true));
    }
}
