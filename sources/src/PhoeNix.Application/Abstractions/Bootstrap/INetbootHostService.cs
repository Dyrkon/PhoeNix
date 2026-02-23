using PhoeNix.Application.Models.Bootstrap;
using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Bootstrap;

public interface INetbootHostService
{
    Task<Result> StartAsync(
        ProvisioningSessionId sessionId,
        BootArtefactDescriptor artefact,
        CancellationToken cancellationToken);

    Task<Result> StopAsync(CancellationToken cancellationToken);

    Task<Result<NetbootHostStatus>> GetStatusAsync(CancellationToken cancellationToken);
}
