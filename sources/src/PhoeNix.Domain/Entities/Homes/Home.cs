using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Homes;

public class Home : AggregateRoot<HomeId>
{
    private readonly List<HomeUser> _users = new();
    private readonly List<HomeModule> _modules = new();

    private Home(HomeId id) : base(id)
    {
    }

    public HomeUser HomeUser { get; private set; }

    public string Name { get; private set; }

    public IReadOnlyList<HomeModule> Modules => _modules;

    public IReadOnlyList<HomeUser> Users => _users;

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

    public Result AddUser(User user)
    {
        if (_users.Any(u => u.UserId == user.Id))
            return Result.Failure(new Error("", "This user has been added to this home already"));

        return HomeUser.Create(new HomeUserId(Guid.NewGuid()), Id, user.Id).Tap(u => _users.Add(u));
    }

    public Result RemoveUser(UserId userId)
    {
        var removeUsers = _users.RemoveAll(u => u.UserId == userId);
        if (removeUsers == 0)
            return Result.Failure(new Error("", $"There is no user with id {userId} in this home"));
        
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

    public Result<HomeBuildResult> Build()
    {
        // TODO not implemented  yet
        return new HomeBuildResult();
    }
}