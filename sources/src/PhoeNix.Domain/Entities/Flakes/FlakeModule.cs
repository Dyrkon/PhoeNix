using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Flakes;

public class FlakeModule : Entity<FlakeModuleId>
{
    private FlakeModule(FlakeModuleId id) : base(id)
    {
    }

    public FlakeId FlakeId { get; private set; }

    public ModuleId ModuleId { get; private set; }

    public Flake Flake { get; private set; }

    public Module Module { get; private set; }

    public static Result<FlakeModule> Create(FlakeModuleId id, FlakeId flakeId, ModuleId moduleId)
    {
        return new FlakeModule(id)
        {
            FlakeId = flakeId,
            ModuleId = moduleId
        };
    }
}