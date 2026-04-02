using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.SystemUsers;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Persistence.Repositories;

internal sealed class UserRepository : RepositoryBase<User, UserId>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<User?> GetByNameAsync(string name, CancellationToken token)
    {
        return await DbContext.Users.SingleOrDefaultAsync(m => m.Name.Contains(name), token);
    }

    public Task<User?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken)
    {
        return DbContext.Users.SingleOrDefaultAsync(x => x.NormalizedName == normalizedName, cancellationToken);
    }

    public Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken)
    {
        return DbContext.Users.AnyAsync(x => x.NormalizedName == normalizedName, cancellationToken);
    }
}