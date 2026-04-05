using Microsoft.AspNetCore.Components;
using PhoeNix.Application.Models.Systems;

namespace PhoeNix.WebAPP.Components.Configurations;

public partial class ConfiguredSystemCard : ComponentBase
{
    [Parameter] [EditorRequired] public ConfiguredSystemResponse System { get; set; } = null!;
}