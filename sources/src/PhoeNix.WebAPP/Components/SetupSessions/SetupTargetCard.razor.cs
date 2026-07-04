using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PhoeNix.Contracts.Setup;

namespace PhoeNix.WebAPP.Components.SetupSessions;

public partial class SetupTargetCard
{
    [Parameter] [EditorRequired] public SetupTargetResponse Target { get; set; } = null!;
    [Parameter] public EventCallback OnRerun { get; set; }
    [Inject] private IJSRuntime JS { get; set; } = null!;

    private string? CleanedDescription => Target.LastErrorDescription is null
        ? null
        : string.Join("\n", Target.LastErrorDescription
            .Split('\n')
            .Where(l => !string.IsNullOrWhiteSpace(l)));

    private async Task CopyErrorToClipboardAsync()
    {
        var text = Target.LastErrorDescription ?? Target.LastErrorCode ?? string.Empty;
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", text);
    }
}