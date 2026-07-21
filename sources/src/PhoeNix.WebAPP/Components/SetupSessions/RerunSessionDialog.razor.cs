using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace PhoeNix.WebAPP.Components.SetupSessions;

public partial class RerunSessionDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter] public int RerunMachineCount { get; set; }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private void Confirm()
    {
        MudDialog.Close(DialogResult.Ok(true));
    }
}
