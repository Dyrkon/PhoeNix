using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Application.Models.Configurations;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using InputResponse = PhoeNix.Application.Models.Inputs.InputResponse;
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

    private async Task OpenAddInputDialogAsync()
    {
        var parameters = new DialogParameters<AddEditInputDialog>
        {
            { x => x.ConfigurationId, ConfigurationId },
            { x => x.ExistingInput, (InputResponse?)null }
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseOnEscapeKey = true };
        var dialog = await DialogService.ShowAsync<AddEditInputDialog>("Add Input", parameters, options);
        var result = await dialog.Result;
        if (result is { Canceled: false })
            await ReloadAsync();
    }

    private async Task OpenEditInputDialogAsync(InputResponse input)
    {
        var parameters = new DialogParameters<AddEditInputDialog>
        {
            { x => x.ConfigurationId, ConfigurationId },
            { x => x.ExistingInput, input }
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseOnEscapeKey = true };
        var dialog = await DialogService.ShowAsync<AddEditInputDialog>("Edit Input", parameters, options);
        var result = await dialog.Result;
        if (result is { Canceled: false })
            await ReloadAsync();
    }

    private async Task RemoveInputAsync(Guid inputId)
    {
        var result = await ConfigurationsApiClient.RemoveConfigurationInputAsync(ConfigurationId, inputId);
        if (result.IsFailure)
        {
            Snackbar.Add("Failed to remove input.", Severity.Error);
            return;
        }

        Snackbar.Add("Input removed.", Severity.Success);
        await ReloadAsync();
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