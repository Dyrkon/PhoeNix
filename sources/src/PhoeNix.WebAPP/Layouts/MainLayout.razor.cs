using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.WebAPP.Shared;
using PhoeNix.WebAPP.States;

namespace PhoeNix.WebAPP.Layouts;

public partial class MainLayout
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    private bool _isDarkMode = false;
    private MudTheme? _theme;
    private readonly UserState _userState = new();

    protected override async Task OnInitializedAsync()
    {
        _userState.Changed += OnUserStateChanged;

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

    private void OnUserStateChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _userState.Changed -= OnUserStateChanged;
    }

    private void DarkModeToggle()
    {
        _isDarkMode = !_isDarkMode;
    }

    public string DarkLightModeButtonIcon => _isDarkMode switch
    {
        true => Icons.Material.Rounded.AutoMode,
        false => Icons.Material.Outlined.DarkMode
    };
}