using PhoeNix.Domain.Entities.SystemUsers;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Domain.Repositories;

public interface IUserRepository : IRepository<User, UserId>
{
    Task<User?> GetByNameAsync(string name, CancellationToken token);
    Task<User?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken);
    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken);
}