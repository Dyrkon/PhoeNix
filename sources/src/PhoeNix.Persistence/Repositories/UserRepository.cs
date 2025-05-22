using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Repositories;

namespace PhoeNix.Persistence.Repositories;

internal sealed class UserRepository : RepositoryBase<User, UserId>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<User?> GetByNameAsync(string name, CancellationToken token)
    {
        return DbContext.Users.SingleOrDefaultAsync(m => m.Name.Contains(name), token);
    }
}