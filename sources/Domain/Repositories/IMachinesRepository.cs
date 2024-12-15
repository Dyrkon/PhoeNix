using System.Net.NetworkInformation;
using Domain.Entities.Machine;

namespace Domain.Repositories;

public interface IMachinesRepository
{
    Task<Machine> GetByMacAddressAsync(PhysicalAddress macAddress, CancellationToken cancellationToken);

    void Add(Machine machine);
}