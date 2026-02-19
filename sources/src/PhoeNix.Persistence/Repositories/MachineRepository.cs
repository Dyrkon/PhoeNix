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
            .AsNoTracking()
            .SingleOrDefaultAsync(
                m => m.Title.ToLower() == title.Trim().ToLower(),
                cancellationToken);
    }
}