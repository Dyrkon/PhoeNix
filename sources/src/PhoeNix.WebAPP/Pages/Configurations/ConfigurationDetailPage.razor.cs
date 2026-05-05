using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using PhoeNix.Contracts.Configurations;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using InputResponse = PhoeNix.Contracts.Inputs.InputResponse;
using PhoeNix.WebAPP.Components.Configurations;

namespace PhoeNix.WebAPP.Pages.Configurations;

public partial class ConfigurationDetailPage : ComponentBase
{
    [Inject] private IConfigurationsApiClient ConfigurationsApiClient { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

    [Parameter] public Guid ConfigurationId { get; set; }

    private ConfigurationResponse? _configuration;
    private bool _isLoading = true;
    private MudExpansionPanels? _sharedModulePanels;
    private MudExpansionPanels? _systemPanels;
    private bool _sharedModulesExpanded;
    private bool _systemsExpanded;

    private async Task ToggleSharedModulesAsync()
    {
        if (_sharedModulePanels is null) return;
        if (_sharedModulesExpanded)
            await _sharedModulePanels.CollapseAllAsync();
        else
            await _sharedModulePanels.ExpandAllAsync();
        _sharedModulesExpanded = !_sharedModulesExpanded;
    }

    private async Task ToggleSystemsAsync()
    {
        if (_systemPanels is null) return;
        if (_systemsExpanded)
            await _systemPanels.CollapseAllAsync();
        else
            await _systemPanels.ExpandAllAsync();
        _systemsExpanded = !_systemsExpanded;
    }

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
        var options = new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true, CloseOnEscapeKey = true };
        var dialog = await DialogService.ShowAsync<AddModuleDialog>("Add Shared Module", parameters, options);
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

    private async Task OpenAddSystemModuleDialogAsync(Guid systemId)
    {
        var parameters = new DialogParameters<AddModuleDialog>
        {
            { x => x.ConfigurationId, ConfigurationId },
            { x => x.SystemId, systemId }
        };
        var options = new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true, CloseOnEscapeKey = true };
        var dialog = await DialogService.ShowAsync<AddModuleDialog>("Add Module", parameters, options);
        var result = await dialog.Result;
        if (result is { Canceled: false })
            await ReloadAsync();
    }

    private async Task OpenEditSystemDialogAsync(Guid systemId, string currentName)
    {
        var parameters = new DialogParameters<EditSystemDialog>
        {
            { x => x.ConfigurationId, ConfigurationId },
            { x => x.SystemId, systemId },
            { x => x.CurrentName, currentName }
        };
        var dialog = await DialogService.ShowAsync<EditSystemDialog>("Rename System", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: false })
            await ReloadAsync();
    }

    private async Task ExportAsync()
    {
        if (_configuration is null)
            return;

        var json = JsonSerializer.Serialize(_configuration, new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        });

        var filename = $"configuration-{SanitizeFilename(_configuration.Title)}.json";
        await JS.InvokeVoidAsync("downloadFile", filename, "application/json", json);
    }

    private static string SanitizeFilename(string name) =>
        string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));

    private async Task ReloadAsync()
    {
        var response = await ConfigurationsApiClient.GetConfigurationAsync(ConfigurationId);
        if (response.IsSuccess && response.Value is not null)
            _configuration = response.Value;
        StateHasChanged();
    }
}