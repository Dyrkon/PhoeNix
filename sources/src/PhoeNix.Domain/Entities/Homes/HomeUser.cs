using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Homes;

public class HomeUser : Entity<HomeUserId>
{
    private HomeUser(HomeUserId id) : base(id)
    {
    }

    public HomeId HomeId { get; private set; }

    public UserId UserId { get; private set; }

    public Home Home { get; private set; }

    public User User { get; private set; }

    public static Result<HomeUser> Create(HomeUserId id, HomeId homeId, UserId userId)
    {
        return new HomeUser(id)
        {
            HomeId = homeId,
            UserId = userId
        };
    }
    
    internal void SetUser(User user)
    {
        User = user;
    }
}