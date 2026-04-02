using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Repositories;

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

    public async Task<IEnumerable<Machine>> GetAllMachines(CancellationToken cancellationToken)
    {
        return await DbContext.Machines.ToListAsync(cancellationToken);
    }
}