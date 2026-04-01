using Microsoft.AspNetCore.Components;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.Extensions;
using PhoeNix.WebAPP.States;

namespace PhoeNix.WebAPP.Components.Navigation;

public partial class LeftPanel : ComponentBase
{
    [Inject] private IAuthenticationApiClient AuthenticationApiClient { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [CascadingParameter] public UserState UserState { get; set; } = null!;

    private bool _drawerOpen = false;

    private async Task Logout()
    {
        await AuthenticationApiClient.LogoutAsync();
        UserState.Clear();
        NavigationManager.NavigateToLogin();
    }
}