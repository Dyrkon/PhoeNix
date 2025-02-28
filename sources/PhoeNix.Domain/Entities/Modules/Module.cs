using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public class Module : AggregateRoot<ModuleId>
{
    private readonly List<Architecture> _supportedArchitectures = new();
    private readonly List<ModuleEntry> _entries = new();

    private Module(ModuleId id) : base(id)
    {
    }

    public string Name { get; private set; }

    public bool Enabled { get; private set; }

    public ModuleType Type { get; private set; }

    public IReadOnlyList<ModuleEntry> Entries => _entries;

    public IReadOnlyList<Architecture> SupportedArchitectures => _supportedArchitectures;

    public Result AddEntry(ModuleEntry entry)
    {
        if (_entries.Any(e => e.Id == entry.Id))
            return Result.Failure(new Error("", $"Entry {entry.Id} is added already"));

        _entries.Add(entry);
        return Result.Success();
    }

    public Result RemoveEntry(ModuleEntryId id)
    {
        var removed = _entries.RemoveAll(e => e.Id == id);
        if (removed == 0)
            return Result.Failure(new Error("", $"There is no entry with Id {id} in module {Name}"));

        return Result.Success();
    }

    public Result Disable()
    {
        if (Enabled == false)
            return Result.Failure(new Error("", $"Module {Name} is already disabled"));
        Enabled = false;
        return Result.Success();
    }

    public Result Enable()
    {
        if (Enabled == true)
            return Result.Failure(new Error("", $"Module {Name} is already enabled"));
        Enabled = true;
        return Result.Success();
    }

    public Result EditModule(string newName)
    {
        if (newName == string.Empty)
            return Result.Failure(new Error("", "Module name can't be empty"));

        Name = newName;
        return Result.Success();
    }

    public Result AddArchitectureSupport(Architecture architecture)
    {
        if (_supportedArchitectures.Contains(architecture))
            return Result.Failure(Error.ValueNotFound);

        _supportedArchitectures.Add(architecture);
        return Result.Success();
    }

    public Result RemoveArchitectureSupport(Architecture architecture)
    {
        var removed = _supportedArchitectures.RemoveAll(a => a == architecture);
        if (removed == 0)
            return Result.Failure(Error.ValueNotFound);

        return Result.Success();
    }

    public static Result<Module> Create(ModuleId id, string name, bool enabled, ModuleType type,
        List<Architecture> architectures)
    {
        if (name == string.Empty)
            return Result.Failure<Module>(new Error("", "Modules name can't be empty"));

        if (architectures.Count == 0)
            return Result.Failure<Module>(new Error("", $"Module has to support at least one architecture"));

        return new Module(id)
        {
            Name = name,
            Enabled = enabled,
            Type = type
        };
    }
}