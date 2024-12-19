using System.Net.NetworkInformation;
using Domain.Entities.Machine;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Data;

namespace PhoeNix.Persistence.Repositories;

public class MachinesRepository(IApplicationDbContext applicationDbContext) : IMachinesRepository
{
    public async Task<Machine?> GetByMacAddressAsync(PhysicalAddress macAddress, CancellationToken cancellationToken)
    {
        return await applicationDbContext.Machines.FirstOrDefaultAsync(machine => machine.MacAddress == macAddress,
            cancellationToken);
    }

    public void Add(Machine machine)
    {
        applicationDbContext.Machines.Add(machine);
    }
}