namespace PhoeNix.Domain.Entities.Modules;

public record ModuleTestBuildResult(
    TestId Id,
    string Content,
    string Name,
    string TestedModulePathPlaceholder,
    string InputsLocationPlaceholder);