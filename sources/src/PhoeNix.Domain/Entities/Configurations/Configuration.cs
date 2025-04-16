using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Configurations;

public class Configuration : AggregateRoot<ConfigurationId>
{
    private readonly List<Input> _inputs = new();
    private readonly List<ConfigurationModule> _modules = new();
    private readonly List<ConfigurationSystem> _systems = new();
    private readonly List<ConfigurationHome> _homes = new();

    public string Title { get; private set; }
    public string Description { get; private set; }
    public IReadOnlyList<Input> Inputs => _inputs;
    public IReadOnlyList<ConfigurationModule> Modules => _modules;
    public IReadOnlyList<ConfigurationSystem> Systems => _systems;
    public IReadOnlyList<ConfigurationHome> Homes => _homes;

    private Configuration(ConfigurationId id) : base(id)
    {
    }

    public Result EditConfiguration(string? newTitle = null, string? newDescription = null)
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
            return Result.Failure(new Error("",
                $"This module ({moduleId}) is added already to this ({Title}) configuration"));

        return ConfigurationModule.Create(new ConfigurationModuleId(Guid.NewGuid()), Id, moduleId)
            .Tap(configurationModule => _modules.Add(configurationModule));
    }

    public Result RemoveModule(ModuleId moduleId)
    {
        var removedModules = _modules.RemoveAll(m => m.ModuleId == moduleId);
        if (removedModules == 0)
            return Result.Failure(new Error("",
                $"There is no module with id {moduleId} in this ({Title}) configuration"));

        return Result.Success();
    }

    public Result AddSystem(SystemId systemId)
    {
        if (_systems.Any(h => h.SystemId == systemId))
            return Result.Failure(new Error("", $"This system ({systemId}) is added already"));

        return ConfigurationSystem.Create(new ConfigurationSystemId(Guid.NewGuid()), Id, systemId)
            .Tap(configurationSystem => _systems.Add(configurationSystem));
    }

    public Result RemoveSystem(SystemId systemId)
    {
        var removedSystems = _systems.RemoveAll(s => s.SystemId == systemId);
        if (removedSystems == 0)
            return Result.Failure(new Error("",
                $"There is no system with id {systemId} in this ({Title}) configuration"));

        return Result.Success();
    }

    public Result AddHome(HomeId homeId)
    {
        if (_homes.Any(h => h.HomeId == homeId))
            return Result.Failure(new Error("", $"This home ({homeId}) is added to this ({Id}) configuration already"));

        return ConfigurationHome.Create(new ConfigurationHomeId(Guid.NewGuid()), Id, homeId)
            .Tap(configurationHome => _homes.Add(configurationHome));
    }

    public Result RemoveHome(HomeId homeId)
    {
        var removeHomes = _homes.RemoveAll(h => h.HomeId == homeId);
        if (removeHomes == 0)
            return Result.Failure(new Error("", $"There is no home with id {homeId} in this ({Title}) configuration"));

        return Result.Success();
    }

    public Result AddInput(Input input)
    {
        if (_inputs.Any(i => i.Id == input.Id))
            return Result.Failure(new Error("",
                $"This input ({input.Name}) is added to this ({Title}) configuration already"));

        _inputs.Add(input);
        return Result.Success();
    }

    public Result RemoveInput(InputId inputId)
    {
        var removeHomes = _inputs.RemoveAll(i => i.Id == inputId);
        if (removeHomes == 0)
            return Result.Failure(new Error("",
                $"There is no input with id {inputId} in this ({Title}) configuration"));

        return Result.Success();
    }

    public Result<List<Architecture>> SupportedSystemArchitectures()
    {
        if (_systems.Count == 0) return new List<Architecture>();
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

    public static Result<Configuration> Create(ConfigurationId id, string title, string description)
    {
        return new Configuration(id) { Title = title, Description = description };
    }
}