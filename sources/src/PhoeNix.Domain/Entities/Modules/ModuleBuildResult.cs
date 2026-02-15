namespace PhoeNix.Domain.Entities.Modules;

public record ModuleBuildResult(
    ModuleTemplateId Id,
    string Name,
    string Module,
    string Inputs,
    string InputsFileName,
    string InputsLocationPlaceholder,
    List<ModuleTestBuildResult>? ModuleTests = null);