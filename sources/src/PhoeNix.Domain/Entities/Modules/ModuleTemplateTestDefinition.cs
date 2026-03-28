namespace PhoeNix.Domain.Entities.Modules;

public sealed record ModuleTemplateTestDefinition(
    TestId? Id,
    string Name,
    string Content,
    IReadOnlyCollection<string> VariableNames);