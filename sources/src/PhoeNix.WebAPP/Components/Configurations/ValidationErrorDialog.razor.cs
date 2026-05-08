using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using PhoeNix.Contracts.Validation;

namespace PhoeNix.WebAPP.Components.Configurations;

public partial class ValidationErrorDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

    [Parameter] public string? ErrorCode { get; set; }
    [Parameter] public string? ErrorMessage { get; set; }
    [Parameter] public List<ModuleTestResultResponse>? TestResults { get; set; }

    private string? CleanedErrorMessage => ErrorMessage is null
        ? null
        : string.Join("\n", ErrorMessage.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)));

    private async Task CopyErrorToClipboardAsync()
    {
        var text = ErrorMessage ?? ErrorCode ?? string.Empty;
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", text);
    }

    private void Close() => MudDialog.Close();
}
