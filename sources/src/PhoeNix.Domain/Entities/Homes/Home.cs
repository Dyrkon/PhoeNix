using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Homes;

public class Home : AggregateRoot<HomeId>
{
    private Home(HomeId id) : base(id)
    {
    }

    private readonly List<HomeModule> _modules = new();

    public HomeUser HomeUser { get; private set; }

    public string Name { get; private set; }

    public IReadOnlyList<HomeModule> Modules => _modules;

    public Result AddModule(Module module)
    {
        if (_modules.Any(m => m.ModuleId == module.Id))
            return Result.Failure(new Error("", "This module has been added to this home already"));

        return HomeModule.Create(new HomeModuleId(Guid.NewGuid()), Id, module.Id)
            .Tap(m => _modules.Add(m));
    }

    public Result RemoveModule(ModuleId moduleId)
    {
        var removeHomes = _modules.RemoveAll(m => m.ModuleId == moduleId);
        if (removeHomes == 0)
            return Result.Failure(new Error("", $"There is no module with id {moduleId} in this home"));

        return Result.Success();
    }

    public static Result<Home> Create(HomeId id, string name)
    {
        return new Home(id) { Name = name };
    }

    internal void SetHomeUser(HomeUser user)
    {
        HomeUser = user;
    }
}