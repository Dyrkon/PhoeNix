using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.SystemUsers;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Repositories;

namespace PhoeNix.Persistence.Repositories;

internal sealed class UserRepository : RepositoryBase<User, UserId>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<SystemUser?> GetByNameAsync(string name, CancellationToken token)
    {
        return await DbContext.SystemUsers.SingleOrDefaultAsync(m => m.Name.Contains(name), token);
    }
}