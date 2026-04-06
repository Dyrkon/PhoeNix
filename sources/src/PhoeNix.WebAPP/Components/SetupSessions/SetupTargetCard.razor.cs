using Microsoft.AspNetCore.Components;
using PhoeNix.Application.Models.Setup;

namespace PhoeNix.WebAPP.Components.SetupSessions;

public partial class SetupTargetCard
{
    [Parameter] [EditorRequired] public SetupTargetResponse Target { get; set; } = null!;
}