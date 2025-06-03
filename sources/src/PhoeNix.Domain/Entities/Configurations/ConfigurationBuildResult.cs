using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Entities.Configurations;

public record ConfigurationBuildResult(
    string Title,
    string Content,
    string SharedModulesPlaceholder,
    string SystemsPlaceholder,
    string ChecksPlaceholder,
    IEnumerable<Architecture> SupportedArchitectures,
    IEnumerable<ModuleBuildResult> CommonModules,
    IEnumerable<SystemBuildResult> Systems);