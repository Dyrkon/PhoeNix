using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Flakes;

public class FlakeSystem : Entity<FlakeSystemId>
{
    private FlakeSystem(FlakeSystemId id) : base(id)
    {
    }

    public FlakeId FlakeId { get; private set; }

    public SystemId SystemId { get; private set; }

    public Flake Flake { get; private set; }

    public Systems.System System { get; private set; }

    public static Result<FlakeSystem> Create(FlakeSystemId id, FlakeId flakeId, SystemId systemId)
    {
        return new FlakeSystem(id)
        {
            FlakeId = flakeId,
            SystemId = systemId
        };
    }
}