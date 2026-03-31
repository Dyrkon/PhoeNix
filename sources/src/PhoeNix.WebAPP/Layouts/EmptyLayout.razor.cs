using MudBlazor;
using PhoeNix.WebAPP.Shared;

namespace PhoeNix.WebAPP.Layouts;

public partial class EmptyLayout
{
    private MudTheme? _theme;

    protected override void OnInitialized()
    {
        _theme = new MudTheme
        {
            PaletteLight = ColorSchemes.LightPalette,
            PaletteDark = ColorSchemes.DarkPalette,
            LayoutProperties = new LayoutProperties()
        };
    }
}