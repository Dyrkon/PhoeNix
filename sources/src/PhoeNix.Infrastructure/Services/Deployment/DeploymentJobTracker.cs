using System.Collections.Concurrent;
using System.Threading.Channels;
using PhoeNix.Application.Abstractions.Deployment;
using PhoeNix.Application.Models.Deployment;
using PhoeNix.Domain.Entities.Machines;

namespace PhoeNix.Infrastructure.Services.Deployment;

internal sealed class DeploymentJobTracker : IDeploymentJobTracker
{
    private readonly Channel<DeploymentJob> _channel = Channel.CreateUnbounded<DeploymentJob>();
    private readonly ConcurrentDictionary<MachineId, DeploymentJobStatus> _statuses = new();

    public ChannelReader<DeploymentJob> Reader => _channel.Reader;

    public void Enqueue(DeploymentJob job)
    {
        _statuses[job.MachineId] = new DeploymentJobStatus(DeploymentJobState.Queued);
        _channel.Writer.TryWrite(job);
    }

    public DeploymentJobStatus GetStatus(MachineId machineId) =>
        _statuses.TryGetValue(machineId, out var status)
            ? status
            : new DeploymentJobStatus(DeploymentJobState.None);

    public void SetStatus(MachineId machineId, DeploymentJobStatus status) =>
        _statuses[machineId] = status;
}
