using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Users;

public class User : AggregateRoot<UserId>
{
    private User(UserId id) : base(id)
    {
    }

    public static Result<User> Create(UserId id)
    {
        return new User(id);
    }
}