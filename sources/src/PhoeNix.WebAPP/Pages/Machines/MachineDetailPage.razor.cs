using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Application.Models.Machines;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.Pages.Machines;

public partial class MachineDetailPage : ComponentBase
{
    [Inject] private IMachinesApiClient MachinesApiClient { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Parameter] public Guid MachineId { get; set; }

    private MachineDetailResponse? _machine;
    private bool _isLoading = true;

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
}