using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Contracts.VmHosts;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.Pages.Machines;

public partial class AssignManagementProfileDialog : ComponentBase
{
    [Inject] private IVmHostsApiClient VmHostsApiClient { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public Guid MachineId { get; set; }

    private List<VmHostListResponse> _vmHosts = [];
    private bool _isLoadingHosts = true;
    private Guid _selectedVmHostId;
    private string _externalId = string.Empty;
    private bool _isSaving;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        var result = await VmHostsApiClient.ListVmHostsAsync();
        if (result is { IsSuccess: true, Value: not null })
            _vmHosts = result.Value.ToList();
        _isLoadingHosts = false;
    }

    private void Cancel() => MudDialog.Cancel();

    private async Task SubmitAsync()
    {
        if (_selectedVmHostId == Guid.Empty || string.IsNullOrWhiteSpace(_externalId))
        {
            _errorMessage = "VM host and external ID are required.";
            return;
        }

        _isSaving = true;
        _errorMessage = null;

        var request = new AssignManagementProfileRequest(_selectedVmHostId, _externalId);
        var result = await VmHostsApiClient.AssignManagementProfileAsync(MachineId, request);

        _isSaving = false;

        if (result.IsFailure)
        {
            _errorMessage = result.Error?.Description ?? "Assignment failed.";
            return;
        }

        MudDialog.Close(DialogResult.Ok(true));
    }
}
