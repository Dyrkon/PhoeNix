using PhoeNix.Application.Models.Bootstrap;
using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Bootstrap;

public interface IBootArtifactBuilder
{
    Task<Result<BootArtefactDescriptor>> BuildAsync(
        BootstrapBuildRequest request,
        IBootstrapProgressSink progress,
        CancellationToken cancellationToken);
}
