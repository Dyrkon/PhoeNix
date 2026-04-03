using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Contracts;

namespace PhoeNix.WebAPP.Pages.Machines;

public partial class MachinesIndexPage
{
    [Inject] private IConfigurationsApiClient ConfigurationsApiClient { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private IEnumerable<ConfigurationListResponse> _configurations = [];
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        var configurationsResponse = await ConfigurationsApiClient.GetConfigurationsAsync();

        if (configurationsResponse.IsFailure)
        {
            Snackbar.Add("Failed to load configurations.", Severity.Error);
            _isLoading = false;
            return;
        }

        _configurations = configurationsResponse.Value ?? [];
        _isLoading = false;
    }
}