using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Homes;

public class HomeModule : Entity<HomeModuleId>
{
    private HomeModule(HomeModuleId id) : base(id)
    {
    }

    public HomeId HomeId { get; private set; }

    public ModuleId ModuleId { get; private set; }

    public Home Home { get; private set; }

    public Module Module { get; private set; }

    public static Result<HomeModule> Create(HomeModuleId id, HomeId homeId, ModuleId moduleId)
    {
        return new HomeModule(id)
        {
            HomeId = homeId,
            ModuleId = moduleId
        };
    }
    
    internal void SetModule(Module module)
    {
        Module = module;
    }
}