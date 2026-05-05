using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Contracts.Configurations;

namespace PhoeNix.WebAPP.Components.Configurations;

public partial class ConfiguredSystemCard : ComponentBase
{
    [Parameter] [EditorRequired] public ConfiguredSystemResponse System { get; set; } = null!;
    [Parameter] [EditorRequired] public Guid ConfigurationId { get; set; }
    [Parameter] public EventCallback OnModuleUpdated { get; set; }

    private MudExpansionPanels? _modulePanels;
    private bool _modulesExpanded;

    private async Task ToggleModulesAsync()
    {
        if (_modulePanels is null) return;
        if (_modulesExpanded)
            await _modulePanels.CollapseAllAsync();
        else
            await _modulePanels.ExpandAllAsync();
        _modulesExpanded = !_modulesExpanded;
    }
}
