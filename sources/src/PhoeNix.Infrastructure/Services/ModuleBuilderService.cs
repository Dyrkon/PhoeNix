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
                files.Add(new TextFile($"{moduleTest.Name}.nix", moduleTest.Content
                    .Replace(moduleTest.InputsLocationPlaceholder, inputsLocationFolder)
                    .Replace(moduleTest.TestedModulePathPlaceholder, $"./{DefaultNames.ModuleName}.nix")));

        return new Folder(moduleBuild.Name, files);
    }

    public Folder BuildSystemModule(SystemBuildResult systemBuild)
    {
        var moduleList = string.Empty;
        var inputsLocationFolder = ".";

        foreach (var module in systemBuild.Modules)
            moduleList += $"./{systemBuild.Name}SystemModules/{module.Name}/{DefaultNames.ModuleName}.nix ";

        var files = new List<FileBase>
        {
            new Folder($"{systemBuild.Name}SystemModules", systemBuild.Modules.Select(BuildModule)),
            new TextFile($"{systemBuild.Name}-{systemBuild.Architecture.ToArchitectureString()}.nix",
                systemBuild.Content.Replace(systemBuild.ModulesListPlaceholder, moduleList))
        };

        foreach (var systemBuildModule in systemBuild.Modules)
            if (systemBuildModule.ModuleTests != null && systemBuildModule.ModuleTests.Any())
                foreach (var moduleTest in systemBuildModule.ModuleTests)
                    files.Add(new TextFile($"{moduleTest.Name}.nix", moduleTest.Content
                        .Replace(moduleTest.InputsLocationPlaceholder, inputsLocationFolder)
                        .Replace(moduleTest.TestedModulePathPlaceholder, $"./{DefaultNames.ModuleName}.nix")));

        return new Folder(systemBuild.Name, files);
    }

    public Folder BuildHomeModule(HomeBuildResult homeBuild)
    {
        throw new NotImplementedException();
    }
}