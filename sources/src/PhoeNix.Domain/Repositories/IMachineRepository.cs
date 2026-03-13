using System.Net.NetworkInformation;
using PhoeNix.Domain.Entities.Machines;

namespace PhoeNix.Domain.Repositories;

public interface IMachineRepository : IRepository<Machine, MachineId>
{
    Task<Machine?> GetByTitleAsync(string title, CancellationToken cancellationToken);

    Task<Machine?> GetByMacAddressAsync(PhysicalAddress macAddress, CancellationToken cancellationToken);
}