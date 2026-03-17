using PhoeNix.Domain.Entities.SystemUsers;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Domain.Repositories;

public interface IUserRepository : IRepository<User, UserId>
{
    Task<SystemUser?> GetByNameAsync(string name, CancellationToken token);
}