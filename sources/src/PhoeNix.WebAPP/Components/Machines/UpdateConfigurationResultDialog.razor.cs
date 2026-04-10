using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace PhoeNix.WebAPP.Components.Machines;

public partial class UpdateConfigurationResultDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public bool IsSuccess { get; set; }
    [Parameter] public string? ErrorCode { get; set; }
    [Parameter] public string? ErrorMessage { get; set; }
    [Parameter] public string? ConfigurationTitle { get; set; }
    [Parameter] public string? SystemName { get; set; }

    private void Close() => MudDialog.Close();
}
