using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Entities.Configurations;

public record ConfigurationBuildResult(string Title, string Description, IEnumerable<Architecture> SupportedArchitectures, IEnumerable<InputBuildResult> Inputs, IEnumerable<ModuleBuildResult> Modules, IEnumerable<SystemBuildResult> Systems);