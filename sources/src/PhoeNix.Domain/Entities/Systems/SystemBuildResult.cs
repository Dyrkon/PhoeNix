using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Entities.Systems;

public record SystemBuildResult(
    SystemId Id,
    string Name,
    Architecture Architecture,
    string Content,
    IEnumerable<ModuleBuildResult> Modules,
    string ModulesListPlaceholder);