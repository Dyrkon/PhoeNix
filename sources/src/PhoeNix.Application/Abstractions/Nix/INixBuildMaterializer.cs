using PhoeNix.Application.Models.Setup;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Nix;

public interface INixBuildMaterializer
{
    Result<ConfigurationBuildResult> MaterializeConfiguration(
        Configuration configuration,
        IEnumerable<ModuleTemplate> templates,
        SystemId? systemId = null,
        BuiltInModuleParameters? builtInModules = null);
}