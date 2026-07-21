using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.VmHosts;

namespace PhoeNix.Persistence.Repositories;

public sealed class MachineRepository : RepositoryBase<Machine, MachineId>, IMachineRepository
{
    public MachineRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Machine?> GetByTitleAsync(string title, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Task.FromResult<Machine?>(null);

        return DbContext
            .Set<Machine>()
            .SingleOrDefaultAsync(
                m => m.Title.ToLower() == title.Trim().ToLower(),
                cancellationToken);
    }

    public Task<Machine?> GetByMacAddressAsync(PhysicalAddress macAddress, CancellationToken cancellationToken)
    {
        return DbContext
            .Set<Machine>()
            .SingleOrDefaultAsync(
                m => m.MacAddress.Equals(macAddress),
                cancellationToken);
    }

    public async Task<IReadOnlyList<Machine>> GetAllByInstalledConfigurationIdAsync(
        ConfigurationId configurationId,
        CancellationToken cancellationToken)
    {
        return await DbContext
            .Set<Machine>()
            .Where(m => m.DeploymentSnapshot != null &&
                        m.DeploymentSnapshot.ConfigurationId == configurationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Machine>> GetAllByVmHostIdAsync(
        VmHostId vmHostId,
        CancellationToken cancellationToken)
    {
        return await DbContext
            .Set<Machine>()
            .Where(m => m.ManagementProfile != null &&
                        m.ManagementProfile.VmHostId == vmHostId)
            .ToListAsync(cancellationToken);
    }
}