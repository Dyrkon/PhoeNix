using PhoeNix.Application.Models.SshIdentity;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Authentication;

public interface IDeploySshKeyProvider
{
    Task<Result<DeploySshAccessMaterial>> GetOrCreateAsync(
        MachineId machineId,
        CancellationToken cancellationToken);

    Task<Result> RevokeAsync(
        MachineId machineId,
        CancellationToken cancellationToken);
}