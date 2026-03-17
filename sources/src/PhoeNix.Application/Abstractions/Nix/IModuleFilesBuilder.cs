using PhoeNix.Application.Models.Files;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;

namespace PhoeNix.Application.Abstractions.Nix;

public interface IModuleFilesBuilder
{
    public Folder BuildModule(ModuleBuildResult moduleBuild);

    public Folder BuildSystemModule(SystemBuildResult systemBuild);
}