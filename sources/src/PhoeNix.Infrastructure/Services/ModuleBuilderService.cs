using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Models.Files;
using PhoeNix.Domain.Services;

namespace PhoeNix.Infrastructure.Services;

public class ModuleBuilderService() : IModuleBuilderService
{
    public Folder BuildModule(ModuleBuildResult moduleBuild)
    {
        var files = new List<FileBase>
        {
            new TextFile("default.nix", moduleBuild.Module.Replace(moduleBuild.InputsLocationPlaceholder, ".")),
            new TextFile($"{moduleBuild.InputsFileName}", moduleBuild.Inputs)
        };

        return new Folder(moduleBuild.Name, files);
    }

    public Folder BuildSystemModule(SystemBuildResult systemBuild)
    {
        var moduleList = string.Empty;

        foreach (var module in systemBuild.Modules)
            moduleList += $"./{systemBuild.Name}SystemModules/{module.Name}/default.nix ";

        var files = new List<FileBase>
        {
            new Folder($"{systemBuild.Name}SystemModules", systemBuild.Modules.Select(BuildModule)),
            new TextFile($"{systemBuild.Name}-{systemBuild.Architecture.ToArchitectureString()}.nix",
                systemBuild.Content.Replace(systemBuild.ModulesListPlaceholder, moduleList))
        };

        return new Folder(systemBuild.Name, files);
    }

    public Folder BuildHomeModule(HomeBuildResult homeBuild)
    {
        throw new NotImplementedException();
    }
}