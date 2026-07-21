using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.VmHosts;

namespace PhoeNix.Persistence.Repositories;

public sealed class VmHostRepository : RepositoryBase<VmHost, VmHostId>, IVmHostRepository
{
    public VmHostRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<VmHost?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Task.FromResult<VmHost?>(null);

        return DbContext
            .Set<VmHost>()
            .SingleOrDefaultAsync(
                h => h.Name.ToLower() == name.Trim().ToLower(),
                cancellationToken);
    }

    public void Remove(VmHost vmHost)
    {
        DbContext.Set<VmHost>().Remove(vmHost);
    }
}
