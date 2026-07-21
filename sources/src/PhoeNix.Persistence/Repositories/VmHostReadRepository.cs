using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Repositories;
using PhoeNix.Contracts.VmHosts;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Entities.VmHosts;

namespace PhoeNix.Persistence.Repositories;

public sealed class VmHostReadRepository(
    ApplicationDbContext dbContext) : IVmHostReadRepository
{
    public async Task<IReadOnlyList<VmHostListResponse>> GetAllAsync(
        UserId ownerId,
        CancellationToken cancellationToken)
    {
        var hosts = await dbContext.Set<VmHost>()
            .AsNoTracking()
            .Where(h => h.OwnerId == ownerId)
            .OrderBy(h => h.Name)
            .ToListAsync(cancellationToken);

        var hostIds = hosts.Select(h => h.Id).ToList();

        var linkedCounts = await dbContext.Set<Machine>()
            .AsNoTracking()
            .Where(m => m.ManagementProfile != null &&
                        hostIds.Contains(m.ManagementProfile.VmHostId))
            .GroupBy(m => m.ManagementProfile!.VmHostId)
            .Select(g => new { VmHostId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.VmHostId, x => x.Count, cancellationToken);

        return hosts.Select(h => new VmHostListResponse(
            h.Id.Value,
            h.Name,
            h.Provider,
            h.Enabled,
            linkedCounts.GetValueOrDefault(h.Id, 0),
            h.Resources is not null
                ? new VmHostResourcesResponse(
                    h.Resources.TotalCpuCores,
                    h.Resources.UsedCpuCores,
                    h.Resources.TotalMemoryMb,
                    h.Resources.UsedMemoryMb,
                    h.Resources.TotalStorageGb,
                    h.Resources.UsedStorageGb)
                : null,
            h.LastSyncedAtUtc)).ToList();
    }

    public async Task<VmHostDetailResponse?> GetByIdAsync(
        Guid vmHostId,
        CancellationToken cancellationToken)
    {
        var id = new VmHostId(vmHostId);
        var host = await dbContext.Set<VmHost>()
            .AsNoTracking()
            .SingleOrDefaultAsync(h => h.Id == id, cancellationToken);

        if (host is null)
            return null;

        var linkedCount = await dbContext.Set<Machine>()
            .AsNoTracking()
            .CountAsync(m => m.ManagementProfile != null &&
                             m.ManagementProfile.VmHostId == id, cancellationToken);

        return new VmHostDetailResponse(
            host.Id.Value,
            host.Name,
            host.Provider,
            host.Enabled,
            host.Credential.Host,
            host.Credential.Port,
            host.Credential.Username,
            host.Credential.ExtraConfig,
            linkedCount,
            host.Resources is not null
                ? new VmHostResourcesResponse(
                    host.Resources.TotalCpuCores,
                    host.Resources.UsedCpuCores,
                    host.Resources.TotalMemoryMb,
                    host.Resources.UsedMemoryMb,
                    host.Resources.TotalStorageGb,
                    host.Resources.UsedStorageGb)
                : null,
            host.LastSyncedAtUtc);
    }
}
