using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Users;

public class User : AggregateRoot<UserId>
{
    private User(UserId id) : base(id)
    {
    }
}