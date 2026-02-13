using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public class NixBuildMaterializer : INixBuildMaterializer
{
    public Result<InputBuildResult> BuildInput(Input input)
    {
        throw new NotImplementedException();
    }

    public Result<ConfigurationBuildResult> BuildConfiguration(Configuration configuration)
    {
        throw new NotImplementedException();
    }

    public Result<SystemBuildResult> BuildSystem(Domain.Entities.Systems.System system)
    {
        throw new NotImplementedException();
    }

    public Result<ModuleBuildResult> BuildModule(Module module)
    {
        throw new NotImplementedException();
    }
}