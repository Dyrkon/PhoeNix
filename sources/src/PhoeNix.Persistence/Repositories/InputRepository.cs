using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Repositories;

namespace PhoeNix.Persistence.Repositories;

internal sealed class InputRepository : RepositoryBase<Input, InputId>, IInputRepository
{
    public InputRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public override Task<Input?> GetByIdAsync(InputId id, CancellationToken token)
    {
        return DbContext.Inputs
            .Include(i => i.Follows)
            .Include(i => i.Followers)
            .SingleOrDefaultAsync(i => i.Id == id, cancellationToken: token);
    }

    public Task<Input?> GetByNameAsync(string name, CancellationToken token)
    {
        return DbContext.Inputs
            .Include(i => i.Follows)
            .Include(i => i.Followers)
            .SingleOrDefaultAsync(i => i.Name.Contains(name), token);
    }

    public async Task<bool> IsSourceUniqueAsync(string source, CancellationToken token)
    {
        return await DbContext.Inputs.CountAsync(i => i.Source == source, token) == 1;
    }
}
