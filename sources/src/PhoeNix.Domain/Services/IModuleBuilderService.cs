using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Models.Files;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Services;

public interface IModuleBuilderService
{
    public Folder BuildModule(ModuleBuildResult moduleBuild);

    public Folder BuildSystemModule(SystemBuildResult systemBuild);

    public Folder BuildHomeModule(HomeBuildResult homeBuild);
}