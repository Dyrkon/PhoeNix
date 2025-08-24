using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Models.Files;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public class ConfigurationBuilderService(IModuleBuilderService moduleBuilderService) : IConfigurationBuilderService
{
    public Result<Folder> BuildConfiguration(ConfigurationBuildResult configurationBuild)
    {
        var configModulesFolderPath = $"{configurationBuild.Title}Modules";
        var configSystemsFolderPath = $"{configurationBuild.Title}Systems";

        var systemPathsList = configurationBuild.Systems.Aggregate("", (current, s) =>
            current +
            $"{s.Name} = import ./{configSystemsFolderPath}/{s.Name}/{s.Name}-{s.Architecture.ToArchitectureString()}.nix {{ inherit inputs sharedModules lib; }};\n");

        var sharedModulesList = configurationBuild.CommonModules.Aggregate("",
            (current, m) => current + $"./{configModulesFolderPath}/{m.Name}/{DefaultNames.ModuleName}.nix");

        var checksPaths = string.Empty;

        foreach (var moduleBuildResult in configurationBuild.CommonModules.Where(m =>
                     m.ModuleTests != null && m.ModuleTests.Count != 0))
            if (moduleBuildResult.ModuleTests != null)
                foreach (var moduleTest in moduleBuildResult.ModuleTests)
                    checksPaths +=
                        $"{moduleTest.Name} = import ./{configModulesFolderPath}/{moduleBuildResult.Name}/{moduleTest.Name}.nix {{ inherit inputs pkgs lib system; }}; ";

        foreach (var system in configurationBuild.Systems)
        foreach (var moduleBuildResult in system.Modules)
            if (moduleBuildResult.ModuleTests != null)
                foreach (var moduleTestBuildResult in moduleBuildResult.ModuleTests)
                    checksPaths +=
                        $"{moduleTestBuildResult.Name} = import ./{configSystemsFolderPath}/{system.Name}/{system.Name}{moduleBuildResult.Name}s/{moduleBuildResult.Name}/{moduleTestBuildResult.Name}.nix {{ inherit inputs pkgs lib system; }}; ";


        var files = new List<FileBase>
        {
            new TextFile("flake.nix",
                configurationBuild.Content
                    .Replace(configurationBuild.SystemsPlaceholder, systemPathsList)
                    .Replace(configurationBuild.SharedModulesPlaceholder, sharedModulesList)
                    .Replace(configurationBuild.ChecksPlaceholder, checksPaths)),
            new Folder(configModulesFolderPath,
                configurationBuild.CommonModules.Select(moduleBuilderService.BuildModule)),
            new Folder(configSystemsFolderPath,
                configurationBuild.Systems.Select(moduleBuilderService.BuildSystemModule))
        };

        return new Folder(configurationBuild.Title, files);
    }
}