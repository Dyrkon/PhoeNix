using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Models.Files;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public class ModuleBuilderService : IModuleBuilderService
{
    private ConfigurationLayout _configurationLayout = new();

    public Folder BuildModule(ModuleBuildResult moduleBuild)
    {
        var inputsLocationFolder = ".";

        var files = new List<FileBase>
        {
            new TextFile($"{DefaultNames.ModuleName}.nix",
                moduleBuild.Module.Replace(moduleBuild.InputsLocationPlaceholder, inputsLocationFolder)),
            new TextFile($"{moduleBuild.InputsFileName}.nix", moduleBuild.Inputs)
        };

        if (moduleBuild.ModuleTests != null && moduleBuild.ModuleTests.Any())
            foreach (var moduleTest in moduleBuild.ModuleTests)
                files.Add(new TextFile($"{moduleTest.Id.ToStringWithPrefix()}.nix", moduleTest.Content
                    .Replace(moduleTest.InputsLocationPlaceholder, inputsLocationFolder)
                    .Replace(moduleTest.TestedModulePathPlaceholder, $"./{DefaultNames.ModuleName}.nix")));

        return new Folder(moduleBuild.Id.ToStringWithPrefix(), files);
    }

    public Folder BuildSystemModule(SystemBuildResult systemBuild)
    {
        var moduleList = string.Empty;
        var inputsLocationFolder = ".";

        foreach (var module in systemBuild.Modules)
            moduleList +=
                $"./{_configurationLayout.SystemModulePath(systemBuild.Id, module.Id)
                    .Replace($"{_configurationLayout.SystemPath(systemBuild.Id)}/", "")} ";


        var files = new List<FileBase>
        {
            new Folder("Modules", systemBuild.Modules.Select(BuildModule)),
            new TextFile(
                _configurationLayout.SystemName(systemBuild.Id, systemBuild.Architecture),
                systemBuild.Content.Replace(systemBuild.ModulesListPlaceholder, moduleList))
        };

        return new Folder(systemBuild.Id.ToStringWithPrefix(), files);
    }

    public Folder BuildHomeModule(HomeBuildResult homeBuild)
    {
        throw new NotImplementedException();
    }
}