using Microsoft.AspNetCore.Components;
using PhoeNix.Contracts.Machines;

namespace PhoeNix.WebAPP.Components.Machines;

public partial class MachineDeploymentSnapshotPanel
{
    [Parameter] [EditorRequired] public DeploymentSnapshotResponse? Snapshot { get; set; }
}