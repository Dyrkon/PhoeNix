namespace PhoeNix.Domain.Entities.Modules;

public record ModuleTestBuildResult(
    string Content,
    string Name,
    string TestedModulePathPlaceholder,
    string InputsLocationPlaceholder);