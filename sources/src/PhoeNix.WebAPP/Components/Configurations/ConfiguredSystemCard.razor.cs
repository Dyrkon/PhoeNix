using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Application.Models.Systems;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.Components.Configurations;

public partial class ConfiguredSystemCard : ComponentBase
{
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private IConfigurationsApiClient ConfigurationsApiClient { get; set; } = null!;

    [Parameter] [EditorRequired] public ConfiguredSystemResponse System { get; set; } = null!;
    [Parameter] [EditorRequired] public Guid ConfigurationId { get; set; }
    [Parameter] public EventCallback OnModuleAdded { get; set; }
    [Parameter] public EventCallback OnModuleUpdated { get; set; }
    [Parameter] public EventCallback OnSystemUpdated { get; set; }

    private async Task OpenAddModuleDialogAsync()
    {
        var parameters = new DialogParameters<AddModuleDialog>
        {
            { x => x.ConfigurationId, ConfigurationId },
            { x => x.SystemId, System.Id }
        };
        var dialog = await DialogService.ShowAsync<AddModuleDialog>("Add Module", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: false })
            await OnModuleAdded.InvokeAsync();
    }

    private async Task OpenEditSystemDialogAsync()
    {
        var parameters = new DialogParameters<EditSystemDialog>
        {
            { x => x.ConfigurationId, ConfigurationId },
            { x => x.SystemId, System.Id },
            { x => x.CurrentName, System.Name }
        };
        var dialog = await DialogService.ShowAsync<EditSystemDialog>("Rename System", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: false })
            await OnSystemUpdated.InvokeAsync();
    }
}