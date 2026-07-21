using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using PhoeNix.Contracts.Modules;
using PhoeNix.WebAPP.Extensions;

namespace PhoeNix.WebAPP.Components.Templates;

public partial class TemplateUpdateImpactDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject] private IJSRuntime JS { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Parameter] public string TemplateName { get; set; } = string.Empty;
    [Parameter] public IReadOnlyList<AffectedConfigurationSummary> AffectedConfigurations { get; set; } = [];

    private async Task CopyUrlAsync(Guid configId)
    {
        var url = NavigationManager.ToAbsoluteUri(AppRoutes.ConfigurationDetail(configId)).ToString();
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", url);
        Snackbar.Add("URL copied to clipboard.", Severity.Info);
    }

    private void Close()
    {
        MudDialog.Close();
    }
}
