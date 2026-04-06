using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace PhoeNix.WebAPP.Components.SetupSessions;

public partial class CancelSessionDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public int RunningMachineCount { get; set; }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private void Confirm()
    {
        MudDialog.Close(DialogResult.Ok(true));
    }
}