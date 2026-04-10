using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Application.Models.Configurations;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.Components.Configurations;

namespace PhoeNix.WebAPP.Pages.Configurations;

public partial class ConfigurationDetailPage : ComponentBase
{
    [Inject] private IConfigurationsApiClient ConfigurationsApiClient { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Parameter] public Guid ConfigurationId { get; set; }

    private ConfigurationResponse? _configuration;
    private bool _isLoading = true;

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;

        var response = await ConfigurationsApiClient.GetConfigurationAsync(ConfigurationId);

        if (response.IsFailure || response.Value is null)
        {
            Snackbar.Add("Failed to load configuration detail.", Severity.Error);
            _configuration = null;
            _isLoading = false;
            return;
        }

        _configuration = response.Value;
        _isLoading = false;
    }

    private async Task OpenAddSharedModuleDialogAsync()
    {
        var parameters = new DialogParameters<AddModuleDialog>
        {
            { x => x.ConfigurationId, ConfigurationId }
        };
        var dialog = await DialogService.ShowAsync<AddModuleDialog>("Add Shared Module", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: false })
            await ReloadAsync();
    }

    private async Task OpenAddSystemDialogAsync()
    {
        var parameters = new DialogParameters<AddSystemDialog>
        {
            { x => x.ConfigurationId, ConfigurationId }
        };
        var dialog = await DialogService.ShowAsync<AddSystemDialog>("Add System", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: false })
            await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        var response = await ConfigurationsApiClient.GetConfigurationAsync(ConfigurationId);
        if (response.IsSuccess && response.Value is not null)
            _configuration = response.Value;
        StateHasChanged();
    }
}