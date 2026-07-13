using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Application.Repositories;

public interface IVmHostReadRepository
{
    Task<IReadOnlyList<VmHostListResponse>> GetAllAsync(
        UserId ownerId,
        CancellationToken cancellationToken);

    Task<VmHostDetailResponse?> GetByIdAsync(
        Guid vmHostId,
        CancellationToken cancellationToken);
}
