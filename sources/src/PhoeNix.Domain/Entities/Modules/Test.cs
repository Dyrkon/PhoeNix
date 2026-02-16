using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Modules;

public class Test : Entity<TestId>
{
    private readonly List<string> _variableNames = [];

    public ModuleTemplateId ModuleTemplateId { get; init; }

    private Test(TestId id) : base(id)
    {
    }

    public string Content { get; private set; }

    public string Name { get; private set; }

    public IReadOnlyList<string> VariableNames => _variableNames;

    public Result ChangeContent(string newContent, List<string> variableNames)
    {
        if (newContent == string.Empty)
            return Result.Failure(new Error("", $"Content of test {Name} can't be empty."));

        if (variableNames.Any(string.IsNullOrEmpty))
            return Result.Failure(new Error("", $"Variable name in content of test {Name} can't be empty."));

        if (variableNames.Any(vn => !newContent.Contains(vn)))
            return Result.Failure(new Error("", $"All variable missing in new content of test {Name}."));

        _variableNames.Clear();
        _variableNames.AddRange(variableNames);
        Content = newContent;

        return Result.Success();
    }

    public static Result<Test> Create(TestId testId, string name)
    {
        return new Test(testId) { Content = string.Empty, Name = name };
    }
}