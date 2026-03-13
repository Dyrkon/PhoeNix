using PhoeNix.Application.Models.Bootstrap;
using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Bootstrap;

public interface IBootstrapImageBuilder
{
    Task<Result<BootstrapImageDescriptor>> BuildAsync(
        Architecture architecture,
        CancellationToken cancellationToken);
}