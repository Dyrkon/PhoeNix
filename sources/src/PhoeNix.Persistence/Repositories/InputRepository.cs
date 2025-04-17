using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Repositories;

namespace PhoeNix.Persistence.Repositories;

internal sealed class InputRepository : Repository<Input, InputId>, IInputRepository
{
    public InputRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Input?> GetByNameAsync(string name, CancellationToken token)
    {
        return DbContext.Inputs.SingleOrDefaultAsync(i => i.Name.Contains(name), cancellationToken: token);
    }

    public async Task<bool> IsSourceUniqueAsync(string source, CancellationToken token)
    {
        return await DbContext.Inputs.CountAsync(i => i.Source == source, cancellationToken: token) == 1;
    }
}