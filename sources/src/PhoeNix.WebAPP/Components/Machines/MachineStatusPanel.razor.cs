using Microsoft.AspNetCore.Components;
using PhoeNix.Application.Models.Machines;

namespace PhoeNix.WebAPP.Components.Machines;

public partial class MachineStatusPanel
{
    [Parameter] [EditorRequired] public MachineStatusResponse Status { get; set; } = null!;
}