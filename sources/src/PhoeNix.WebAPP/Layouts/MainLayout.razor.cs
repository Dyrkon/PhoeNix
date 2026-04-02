using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.Extensions;
using PhoeNix.WebAPP.Shared;
using PhoeNix.WebAPP.States;

namespace PhoeNix.WebAPP.Layouts;

public partial class MainLayout
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IAuthenticationInvalidationNotifier AuthenticationInvalidationNotifier { get; set; } = null!;

    private bool _isDarkMode;
    private MudTheme? _theme;
    private readonly UserState _userState = new();

    protected override async Task OnInitializedAsync()
    {
        _userState.Changed += OnUserStateChanged;
        AuthenticationInvalidationNotifier.AuthenticationInvalidated += OnAuthenticationInvalidated;

        _theme = new MudTheme
        {
            PaletteLight = ColorSchemes.LightPalette,
            PaletteDark = ColorSchemes.DarkPalette,
            LayoutProperties = new LayoutProperties()
        };

        try
        {
            var result = await AuthenticationApiClient.GetCurrentUserAsync();

            if (result is { IsSuccess: true, Value: not null })
                _userState.SetCurrentUser(result.Value);
            else
                _userState.MarkInitialized();
        }
        catch
        {
            _userState.MarkInitialized();
        }
    }

    private void OnAuthenticationInvalidated()
    {
        _userState.Clear();

        _ = InvokeAsync(() =>
        {
            var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);

            if (!relativePath.StartsWith("login", StringComparison.OrdinalIgnoreCase) &&
                !relativePath.StartsWith("register", StringComparison.OrdinalIgnoreCase))
                NavigationManager.NavigateToLogin();

            StateHasChanged();
        });
    }

    private void OnUserStateChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _userState.Changed -= OnUserStateChanged;
        AuthenticationInvalidationNotifier.AuthenticationInvalidated -= OnAuthenticationInvalidated;
    }
}