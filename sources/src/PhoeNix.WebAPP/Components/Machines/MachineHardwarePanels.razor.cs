using Microsoft.AspNetCore.Components;
using PhoeNix.Application.Models.Machines;

namespace PhoeNix.WebAPP.Components.Machines;

public partial class MachineHardwarePanels
{
    [Parameter] [EditorRequired] public HardwareProfileResponse? Hardware { get; set; }
}