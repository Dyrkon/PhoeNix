using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Application.Models.Configurations;

namespace PhoeNix.WebAPP.Components.Configurations;

public partial class ConfiguredModuleCard : ComponentBase
{
    [Inject] private IDialogService DialogService { get; set; } = null!;

    [Parameter] [EditorRequired] public ConfiguredModuleResponse Module { get; set; } = null!;
    [Parameter] [EditorRequired] public Guid ConfigurationId { get; set; }
    [Parameter] public Guid? SystemId { get; set; }
    [Parameter] public EventCallback OnModuleUpdated { get; set; }

    private async Task OpenEditDialogAsync()
    {
        var parameters = new DialogParameters<EditModuleValuesDialog>
        {
            { x => x.ConfigurationId, ConfigurationId },
            { x => x.SystemId, SystemId },
            { x => x.Module, Module }
        };
        var dialog = await DialogService.ShowAsync<EditModuleValuesDialog>(
            $"Edit: {Module.TemplateName}", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: false })
            await OnModuleUpdated.InvokeAsync();
    }

    private static string ResolveEntryValue(ConfiguredModuleEntryResponse entry)
    {
        if (entry.ListItems is { Count: > 0 })
            return string.Join(", ", entry.ListItems);

        if (!string.IsNullOrWhiteSpace(entry.Value))
            return entry.Value;

        if (entry.IntegerLowerValue.HasValue || entry.IntegerUpperValue.HasValue)
            return $"{entry.IntegerLowerValue?.ToString() ?? "-"} - {entry.IntegerUpperValue?.ToString() ?? "-"}";

        if (entry.DecimalLowerValue.HasValue || entry.DecimalUpperValue.HasValue)
            return $"{entry.DecimalLowerValue?.ToString() ?? "-"} - {entry.DecimalUpperValue?.ToString() ?? "-"}";

        return "-";
    }
}