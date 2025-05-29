namespace PhoeNix.Domain.Entities.Modules;

public record ModuleBuildResult(
    string Name,
    string Module,
    string Inputs,
    string InputsFileName,
    string InputsLocationPlaceholder);