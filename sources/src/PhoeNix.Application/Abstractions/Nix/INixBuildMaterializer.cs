using PhoeNix.Application.Models.Modules;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Nix;

public interface INixBuildMaterializer
{
    Result<ConfigurationBuildResult> MaterializeConfiguration(
        Configuration configuration,
        IEnumerable<ModuleTemplate> templates,
        SystemId? systemId = null,
        BuiltInModuleParameters? builtInModules = null);

    NixModuleScaffolding GetModuleScaffolding(ModuleType type, string? argsImportValue = "./values.nix");

    NixTestScaffolding GetTestScaffolding(string testName, string? testedModule = "./module.nix",
        string? argsInputLocation = "./values.nix");
}