using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Flakes;

public class Flake : Entity<FlakeId>
{
    private readonly List<Input> _inputs = new();
    private readonly List<FlakeModule> _modules = new();
    private readonly List<FlakeSystem> _systems = new();
    private readonly List<FlakeHome> _homes = new();

    public string Title { get; private set; }
    public string Description { get; private set; }
    public IReadOnlyList<Input> Inputs => _inputs;
    public IReadOnlyList<FlakeModule> Modules => _modules;
    public IReadOnlyList<FlakeSystem> Systems => _systems;
    public IReadOnlyList<FlakeHome> Homes => _homes;

    private Flake(FlakeId id) : base(id)
    {
    }

    public Result EditFlake(string? newTitle = null, string? newDescription = null)
    {
        if (newDescription is not null)
        {
            if (newDescription == string.Empty)
                return Result.Failure(new Error("", "Title can't be blank"));

            Description = newDescription;
        }

        if (newTitle is not null)
        {
            if (newTitle == string.Empty)
                return Result.Failure(new Error("", "Title can't be blank"));

            Title = newTitle;
        }

        return Result.Success();
    }


    public Result AddModule(ModuleId moduleId)
    {
        if (_modules.Any(h => h.ModuleId == moduleId))
            return Result.Failure(new Error("", $"This home ({moduleId}) is added already to this ({Title}) flake"));

        return FlakeModule.Create(new FlakeModuleId(Guid.NewGuid()), Id, moduleId)
            .Tap(flakeModule => _modules.Add(flakeModule));
    }

    public Result RemoveModule(ModuleId moduleId)
    {
        var removedModules = _modules.RemoveAll(m => m.ModuleId == moduleId);
        if (removedModules == 0)
            return Result.Failure(new Error("", $"There is no module with id {moduleId} in this ({Title}) flake"));

        return Result.Success();
    }

    public Result AddSystem(SystemId systemId)
    {
        if (_systems.Any(h => h.SystemId == systemId))
            return Result.Failure(new Error("", $"This system ({systemId}) is added already"));

        return FlakeSystem.Create(new FlakeSystemId(Guid.NewGuid()), Id, systemId)
            .Tap(flakeSystem => _systems.Add(flakeSystem));
    }

    public Result RemoveSystem(SystemId systemId)
    {
        var removedSystems = _systems.RemoveAll(s => s.SystemId == systemId);
        if (removedSystems == 0)
            return Result.Failure(new Error("", $"There is no system with id {systemId} in this ({Title}) flake"));

        return Result.Success();
    }

    public Result AddHome(HomeId homeId)
    {
        if (_homes.Any(h => h.HomeId == homeId))
            return Result.Failure(new Error("", $"This home ({homeId}) is added to this ({Id}) flake already"));

        return FlakeHome.Create(new FlakeHomeId(Guid.NewGuid()), Id, homeId)
            .Tap(flakeHome => _homes.Add(flakeHome));
    }

    public Result RemoveHome(HomeId homeId)
    {
        var removeHomes = _homes.RemoveAll(h => h.HomeId == homeId);
        if (removeHomes == 0)
            return Result.Failure(new Error("", $"There is no home with id {homeId} in this ({Title}) flake"));

        return Result.Success();
    }

    public Result AddInput(Input input)
    {
        if (_inputs.Any(i => i.Id == input.Id))
            return Result.Failure(new Error("", $"This input ({input.Name}) is added to this ({Title}) flake already"));

        _inputs.Add(input);
        return Result.Success();
    }

    public Result RemoveInput(InputId inputId)
    {
        var removeHomes = _inputs.RemoveAll(i => i.Id == inputId);
        if (removeHomes == 0)
            return Result.Failure(new Error("", $"There is no input with id {inputId} in this ({Title}) flake"));

        return Result.Success();
    }

    public Result<List<Architecture>> SupportedSystemArchitectures()
    {
        List<Architecture> supportedArchitectures = new();
        if (_systems.All(s => s.System.Architecture == Architecture.X86Linux))
            supportedArchitectures.Add(Architecture.X86Linux);
        if (_systems.All(s => s.System.Architecture == Architecture.Aarch64Linux))
            supportedArchitectures.Add(Architecture.Aarch64Linux);
        if (_systems.All(s => s.System.Architecture == Architecture.X86Darwin))
            supportedArchitectures.Add(Architecture.X86Darwin);
        if (_systems.All(s => s.System.Architecture == Architecture.Aarch64Darwin))
            supportedArchitectures.Add(Architecture.Aarch64Darwin);

        return supportedArchitectures;
    }
}