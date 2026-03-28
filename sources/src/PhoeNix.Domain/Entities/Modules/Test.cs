using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public sealed class Test : Entity<TestId>
{
    private readonly List<string> _variableNames = [];

    public ModuleTemplateId ModuleTemplateId { get; init; } = default!;

    private Test(TestId id) : base(id)
    {
    }

    public string Content { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public IReadOnlyList<string> VariableNames => _variableNames;

    public Result Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure(new Error("Modules.TestNameEmpty", "Module test name can't be empty."));

        Name = newName.Trim();
        return Result.Success();
    }

    public Result ChangeContent(string newContent, List<string> variableNames)
    {
        if (string.IsNullOrWhiteSpace(newContent))
            return Result.Failure(
                new Error("Modules.TestContentEmpty", $"Content of test '{Name}' can't be empty."));

        if (variableNames.Any(string.IsNullOrWhiteSpace))
            return Result.Failure(
                new Error("Modules.TestVariableNameEmpty",
                    $"Variable name in content of test '{Name}' can't be empty."));

        if (variableNames.Any(vn => !newContent.Contains(vn, StringComparison.Ordinal)))
            return Result.Failure(
                new Error("Modules.TestVariableMissing",
                    $"All variables must be present in content of test '{Name}'."));

        _variableNames.Clear();
        _variableNames.AddRange(variableNames);
        Content = newContent;

        return Result.Success();
    }

    public static Result<Test> Create(TestId testId, ModuleTemplateId moduleTemplateId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Test>(
                new Error("Modules.TestNameEmpty", "Module test name can't be empty."));

        return new Test(testId)
        {
            ModuleTemplateId = moduleTemplateId,
            Content = string.Empty,
            Name = name.Trim()
        };
    }
}