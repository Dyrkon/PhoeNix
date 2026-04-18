using System.Threading.Channels;
using PhoeNix.Application.Models.Deployment;
using PhoeNix.Domain.Entities.Machines;

namespace PhoeNix.Application.Abstractions.Deployment;

public interface IDeploymentJobTracker
{
    void Enqueue(DeploymentJob job);
    DeploymentJobStatus GetStatus(MachineId machineId);
    void SetStatus(MachineId machineId, DeploymentJobStatus status);
    ChannelReader<DeploymentJob> Reader { get; }
}
