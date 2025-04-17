using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Domain.Repositories;

public interface IUserRepository : IRepository<User, UserId>
{
    
}