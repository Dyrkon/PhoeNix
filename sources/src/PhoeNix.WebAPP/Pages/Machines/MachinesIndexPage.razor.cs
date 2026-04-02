using Microsoft.AspNetCore.Components;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Contracts;

namespace PhoeNix.WebAPP.Pages.Machines;

public partial class MachinesIndexPage
{
    [Inject] private IMachinesApiClient MachinesApiClient { get; set; } = null!;

    private IEnumerable<MachineListResponse> _machineListResponses = [];
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        var response = await MachinesApiClient.GetMachinesAsync();

        if (response.IsFailure)
        {
            _errorMessage = response.Error?.Description ?? "Failed to load machines.";
            return;
        }

        _machineListResponses = response.Value ?? [];
    }
}