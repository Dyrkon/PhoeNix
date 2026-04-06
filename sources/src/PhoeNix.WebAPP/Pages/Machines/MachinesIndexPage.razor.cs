using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Application.Models.Configurations;
using PhoeNix.Common.Models;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.Pages.Machines;

public partial class MachinesIndexPage
{
    [Inject] private IConfigurationsApiClient ConfigurationsApiClient { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private List<ConfigurationListResponse> _configurations = [];
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        var configurationsResponse =
            await ConfigurationsApiClient.GetConfigurationsAsync(new ListConfigurationsRequest(),
                CancellationToken.None);

        if (configurationsResponse.IsFailure)
        {
            Snackbar.Add("Failed to load configurations.", Severity.Error);
            _isLoading = false;
            return;
        }

        _configurations = configurationsResponse.Value?.Items.ToList() ?? [];
        _isLoading = false;
    }
}