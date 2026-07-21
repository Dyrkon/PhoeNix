using System.Net.NetworkInformation;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.VmHosts;

namespace PhoeNix.Application.Repositories;

public interface IMachineRepository : IRepository<Machine, MachineId>
{
    Task<Machine?> GetByTitleAsync(string title, CancellationToken cancellationToken);
    Task<Machine?> GetByMacAddressAsync(PhysicalAddress macAddress, CancellationToken cancellationToken);

    Task<IReadOnlyList<Machine>> GetAllByInstalledConfigurationIdAsync(
        ConfigurationId configurationId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Machine>> GetAllByVmHostIdAsync(
        VmHostId vmHostId,
        CancellationToken cancellationToken);
}