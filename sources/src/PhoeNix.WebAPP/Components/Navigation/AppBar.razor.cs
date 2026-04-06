using Microsoft.AspNetCore.Components;
using PhoeNix.WebAPP.States;

namespace PhoeNix.WebAPP.Components.Navigation;

public partial class AppBar : ComponentBase, IDisposable
{
    [CascadingParameter] public SetupSessionsState SetupSessionsState { get; set; } = null!;

    protected override void OnInitialized()
    {
        SetupSessionsState.StateChanged += OnStateChanged;
        SetupSessionsState.StartPolling();
    }

    private void OnStateChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        SetupSessionsState.StateChanged -= OnStateChanged;
    }
}