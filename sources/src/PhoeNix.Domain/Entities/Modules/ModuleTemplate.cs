using System.Text.RegularExpressions;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public sealed class ModuleTemplate : AggregateRoot<ModuleTemplateId>
{
    private readonly List<Architecture> _supportedArchitectures = [];
    private readonly List<Test> _tests = [];
    private readonly List<EntryValueDefinition> _editableValueTypes = [];
    private readonly List<RequiredInputDefinition> _requiredInputs = [];

    private ModuleTemplate(ModuleTemplateId id) : base(id)
    {
    }

    public UserId OwnerId { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public bool Enabled { get; private set; }

    public ModuleType Type { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public IReadOnlyList<Test> Tests => _tests;

    public IReadOnlyList<EntryValueDefinition> EditableValueTypes => _editableValueTypes;

    public IReadOnlyList<Architecture> SupportedArchitectures => _supportedArchitectures;

    public IReadOnlyList<RequiredInputDefinition> RequiredInputs => _requiredInputs;

    public bool RequiresSetupBindings =>
        _editableValueTypes.Any(v => v.BindingKind == EntryBindingKind.RankedDiskCandidate);

    public Result EditModule(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure(new Error("Modules.NameEmpty", "Module template name can't be empty."));

        Name = newName.Trim();
        return Result.Success();
    }

    public Result SetEnabled(bool enabled)
    {
        Enabled = enabled;
        return Result.Success();
    }

    public Result ChangeType(ModuleType type)
    {
        Type = type;
        return Result.Success();
    }

    public Result AddArchitectureSupport(Architecture architecture)
    {
        if (_supportedArchitectures.Contains(architecture))
            return Result.Failure(new Error("Modules.ArchitectureAlreadySupported",
                $"Architecture '{architecture}' is already supported."));

        _supportedArchitectures.Add(architecture);
        return Result.Success();
    }

    public Result ReplaceArchitectureSupport(IEnumerable<Architecture> architectures)
    {
        var incomingArchitectures = architectures.Distinct().ToList();

        if (incomingArchitectures.Count == 0)
            return Result.Failure(new Error("Modules.NoArchitecture",
                "Module template has to support at least one architecture."));

        _supportedArchitectures.Clear();
        _supportedArchitectures.AddRange(incomingArchitectures);

        return Result.Success();
    }

    public Result SetRequiredInputs(IEnumerable<(string Name, string Source)> inputs)
    {
        _requiredInputs.Clear();
        _requiredInputs.AddRange(inputs.Select(i => RequiredInputDefinition.Create(Id, i.Name, i.Source)));
        return Result.Success();
    }

    public Result ChangeContent(string content, IReadOnlyCollection<EntryValueDefinition> entries)
    {
        if (string.IsNullOrWhiteSpace(content))
            return Result.Failure(new Error("Modules.ContentEmpty", "Module template content can't be empty."));

        if (entries.GroupBy(x => x.Name, StringComparer.Ordinal).Any(g => g.Count() > 1))
            return Result.Failure(new Error("Modules.DuplicateEntryName",
                "Entry names must be unique within a module template."));

        if (entries.GroupBy(x => x.Placeholder, StringComparer.Ordinal).Any(g => g.Count() > 1))
            return Result.Failure(new Error("Modules.DuplicatePlaceholder",
                "Entry placeholders must be unique within a module template."));

        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Name))
                return Result.Failure(new Error("Modules.EntryNameEmpty", "Entry name can't be empty."));

            if (string.IsNullOrWhiteSpace(entry.Placeholder))
                return Result.Failure(new Error("Modules.PlaceholderEmpty", "Entry placeholder can't be empty."));

            if (!content.Contains(entry.Placeholder, StringComparison.Ordinal))
                return Result.Failure(new Error("Modules.PlaceholderMissing",
                    $"Placeholder '{entry.Placeholder}' is not present in the module content."));

            if (entry.BindingKind == EntryBindingKind.RankedDiskCandidate && entry.BindingIndex is null)
                return Result.Failure(new Error("Modules.BindingIndexMissing",
                    $"Binding index is required for '{EntryBindingKind.RankedDiskCandidate}'."));

            var validation = ValidateEntryDefinition(entry);
            if (validation.IsFailure)
                return validation;
        }

        Content = content;
        _editableValueTypes.Clear();
        _editableValueTypes.AddRange(entries);

        return Result.Success();
    }

    private static readonly Regex NixIdentifierRegex =
        new(@"^[a-zA-Z_][a-zA-Z0-9_-]*$", RegexOptions.Compiled);

    private static Result ValidateEntryDefinition(EntryValueDefinition entry)
    {
        if (!NixIdentifierRegex.IsMatch(entry.Placeholder))
            return Result.Failure(new Error("Modules.PlaceholderNotNixValid",
                $"Placeholder '{entry.Placeholder}' must be a valid Nix identifier " +
                "(letters, digits, underscores, dashes; must not start with a digit)."));

        return entry.ValueKind switch
        {
            EntryValueKind.Text => Result.Success(),

            EntryValueKind.IntegerRange when entry.IntegerMin is null || entry.IntegerMax is null =>
                Result.Failure(new Error("Modules.IntegerRangeDefinitionInvalid",
                    $"Entry '{entry.Name}' requires IntegerMin and IntegerMax.")),

            EntryValueKind.IntegerRange when entry.IntegerMax < entry.IntegerMin =>
                Result.Failure(new Error("Modules.IntegerRangeDefinitionInvalid",
                    $"Entry '{entry.Name}' has invalid integer range bounds.")),

            EntryValueKind.DecimalRange when entry.DecimalMin is null || entry.DecimalMax is null =>
                Result.Failure(new Error("Modules.DecimalRangeDefinitionInvalid",
                    $"Entry '{entry.Name}' requires DecimalMin and DecimalMax.")),

            EntryValueKind.DecimalRange when entry.DecimalMax < entry.DecimalMin =>
                Result.Failure(new Error("Modules.DecimalRangeDefinitionInvalid",
                    $"Entry '{entry.Name}' has invalid decimal range bounds.")),

            EntryValueKind.SingleChoice when entry.GetOptions().Count == 0 =>
                Result.Failure(new Error("Modules.SingleChoiceDefinitionInvalid",
                    $"Entry '{entry.Name}' requires at least one option.")),

            _ => Result.Success()
        };
    }

    public Result AddModuleTest(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(new Error("Modules.TestNameEmpty", "Module test name can't be empty."));

        if (_tests.Any(h => h.Name == name))
            return Result.Failure(new Error("Modules.TestNameDuplicate",
                $"Module test with name '{name}' already exists."));

        return Test.Create(new TestId(Guid.NewGuid()), Id, name.Trim())
            .Tap(t => _tests.Add(t));
    }

    public Result ChangeModuleTest(TestId testId, string newContent, List<string> variableNames)
    {
        return _tests.FirstOrDefault(h => h.Id == testId)
            .EnsureNotNull(new Error("Modules.TestNotFound",
                $"Module test '{testId.Value}' has not been found in module template '{Name}'."))
            .Bind(test => test.ChangeContent(newContent, variableNames));
    }

    public Result RemoveModuleTest(TestId id)
    {
        var removed = _tests.RemoveAll(t => t.Id == id);

        return removed == 0
            ? Result.Failure(new Error("Modules.TestNotFound",
                $"There is no module test with id '{id.Value}' in this module template."))
            : Result.Success();
    }

    public Result ReconcileTests(IReadOnlyCollection<ModuleTemplateTestDefinition> tests)
    {
        if (tests.GroupBy(x => x.Name, StringComparer.Ordinal).Any(g => g.Count() > 1))
            return Result.Failure(new Error("Modules.TestNameDuplicate",
                "Module test names must be unique within a module template."));

        if (tests.Where(x => x.Id is not null).GroupBy(x => x.Id).Any(g => g.Count() > 1))
            return Result.Failure(new Error("Modules.TestIdDuplicate",
                "Module test ids must be unique within a module template update."));

        var requestedIds = tests
            .Where(x => x.Id is not null)
            .Select(x => x.Id!)
            .ToHashSet();

        var testsToRemove = _tests
            .Where(x => !requestedIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToList();

        foreach (var testId in testsToRemove)
        {
            var removeResult = RemoveModuleTest(testId);
            if (removeResult.IsFailure)
                return removeResult;
        }

        foreach (var requestedTest in tests.Where(x => x.Id is not null))
        {
            var existingTest = _tests.FirstOrDefault(x => x.Id == requestedTest.Id);

            if (existingTest is null)
                return Result.Failure(new Error("Modules.TestNotFound",
                    $"Module test '{requestedTest.Id!.Value}' was not found."));

            var renameResult = existingTest.Rename(requestedTest.Name);
            if (renameResult.IsFailure)
                return renameResult;

            var contentResult = existingTest.ChangeContent(requestedTest.Content, requestedTest.VariableNames.ToList());
            if (contentResult.IsFailure)
                return contentResult;
        }

        foreach (var requestedTest in tests.Where(x => x.Id is null))
        {
            var addResult = AddModuleTest(requestedTest.Name);
            if (addResult.IsFailure)
                return addResult;

            var createdTest = _tests.Single(x => x.Name == requestedTest.Name);

            var contentResult = createdTest.ChangeContent(requestedTest.Content, requestedTest.VariableNames.ToList());
            if (contentResult.IsFailure)
                return contentResult;
        }

        return Result.Success();
    }

    public static Result<ModuleTemplate> Create(
        ModuleTemplateId templateId,
        UserId ownerId,
        string name,
        bool enabled,
        ModuleType type,
        IReadOnlyCollection<Architecture> architectures)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<ModuleTemplate>(
                new Error("Modules.NameEmpty", "Module template name can't be empty."));

        if (architectures.Count == 0)
            return Result.Failure<ModuleTemplate>(new Error("Modules.NoArchitecture",
                "Module template has to support at least one architecture."));

        var newModule = new ModuleTemplate(templateId)
        {
            OwnerId = ownerId,
            Name = name.Trim(),
            Enabled = enabled,
            Type = type,
            Content = string.Empty
        };

        var result = newModule.ReplaceArchitectureSupport(architectures);
        return result.IsFailure
            ? Result.Failure<ModuleTemplate>(result.Error)
            : newModule;
    }
}