using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Entities.Systems;

public record SystemBuildResult(Architecture Architecture, string Content, IEnumerable<ModuleBuildResult> Modules, string ModulesListPlaceholder);