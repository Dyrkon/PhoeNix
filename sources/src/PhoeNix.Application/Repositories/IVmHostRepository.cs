using PhoeNix.Domain.Entities.VmHosts;

namespace PhoeNix.Application.Repositories;

public interface IVmHostRepository : IRepository<VmHost, VmHostId>
{
    Task<VmHost?> GetByNameAsync(string name, CancellationToken cancellationToken);

    void Remove(VmHost vmHost);
}
