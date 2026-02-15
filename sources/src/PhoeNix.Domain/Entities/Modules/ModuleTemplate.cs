using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public class ModuleTemplate : AggregateRoot<ModuleTemplateId>
{
    private readonly List<Architecture> _supportedArchitectures = new();
    private readonly List<Test> _tests = new();
    private readonly List<EntryValueDefinition> _editableValueTypes = new();

    private ModuleTemplate(ModuleTemplateId id) : base(id)
    {
    }

    public string Name { get; private set; }

    public ModuleType Type { get; private set; }

    public string Content { get; private set; }

    public IReadOnlyList<Test> Tests => _tests;
    public IReadOnlyList<EntryValueDefinition> EditableValueTypes => _editableValueTypes;

    public Result ChangeContent(string content, List<EntryValueDefinition> entries)
    {
        foreach (var entryValue in entries.Where(entryValue => !content.Contains(entryValue.Name)))
            return Result.Failure(new Error("", $"Name for value {entryValue.Name} is not present"));

        Content = content;
        _editableValueTypes.Clear();
        _editableValueTypes.AddRange(entries);

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
            return Result.Failure(new Error("", $"Can't add already supported architecture {architecture}"));

        _supportedArchitectures.Add(architecture);
        return Result.Success();
    }

    public Result AddArchitecturesSupport(IEnumerable<Architecture> architectures)
    {
        if (_supportedArchitectures.Any(architectures.Contains))
            return Result.Failure(new Error("", "Can't add already supported architectures"));

        _supportedArchitectures.AddRange(architectures);
        return Result.Success();
    }

    public Result RemoveArchitectureSupport(Architecture architecture)
    {
        var removed = _supportedArchitectures.RemoveAll(a => a == architecture);
        return removed == 0 ? Result.Failure(Error.ValueNotFound) : Result.Success();
    }

    public Result AddModuleTest(string name)
    {
        if (_tests.Any(h => h.Name == name))
            return Result.Failure(new Error("", $"Module {name} has with the name {name} already"));

        return Test.Create(new TestId(Guid.NewGuid()), name)
            .Tap(t => _tests.Add(t));
    }


    public Result ChangeModuleTest(TestId testId, string newContent, List<string> variableNames)
    {
        return _tests.FirstOrDefault(h => h.Id == testId)
            .EnsureNotNull(new Error("", $"Module test {Id.Value} has not been found in module template {Name}"))
            .Bind(test => test.ChangeContent(newContent, variableNames));
    }

    public Result RemoveModuleTest(TestId id)
    {
        var removedModules = _tests.RemoveAll(t => t.Id == id);
        if (removedModules == 0)
            return Result.Failure(new Error("", $"There is no module with id {id.Value} in this module"));

        return Result.Success();
    }

    public static Result<ModuleTemplate> Create(ModuleTemplateId templateId, string name, bool enabled, ModuleType type,
        List<Architecture> architectures)
    {
        if (name == string.Empty)
            return Result.Failure<ModuleTemplate>(new Error("", "Modules name can't be empty"));

        if (architectures.Count == 0)
            return Result.Failure<ModuleTemplate>(new Error("", "Module has to support at least one architecture"));

        var newModule = new ModuleTemplate(templateId)
        {
            Name = name,
            Type = type,
            Content = string.Empty
        };
        var result = newModule.AddArchitecturesSupport(architectures);
        return result.IsFailure ? Result.Failure<ModuleTemplate>(result.Error) : newModule;
    }

    public IReadOnlyList<Architecture> SupportedArchitectures => _supportedArchitectures;
}