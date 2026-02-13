using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Models.Files;

namespace PhoeNix.Application.Abstractions.Nix;

public interface IModuleFilesBuilder
{
    public Folder BuildModule(ModuleBuildResult moduleBuild);

    public Folder BuildSystemModule(SystemBuildResult systemBuild);
}