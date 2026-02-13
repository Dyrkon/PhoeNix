using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Nix;

public interface INixBuildMaterializer
{
    public Result<InputBuildResult> BuildInput(Input input);

    public Result<ConfigurationBuildResult> BuildConfiguration(Configuration configuration);

    public Result<SystemBuildResult> BuildSystem(Domain.Entities.Systems.System system);

    public Result<ModuleBuildResult> BuildModule(Module module);
}