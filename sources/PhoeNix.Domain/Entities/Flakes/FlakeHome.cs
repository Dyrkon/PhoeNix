using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Flakes;

public class FlakeHome : Entity<FlakeHomeId>
{
    private FlakeHome(FlakeHomeId id) : base(id)
    {
    }

    public FlakeId FlakeId { get; private set; }

    public HomeId HomeId { get; private set; }

    public Flake Flake { get; private set; }

    public Home Home { get; private set; }

    public static Result<FlakeHome> Create(FlakeHomeId id, FlakeId flakeId, HomeId flakeHomeId)
    {
        return new FlakeHome(id)
        {
            FlakeId = flakeId,
            HomeId = flakeHomeId
        };
    }
}