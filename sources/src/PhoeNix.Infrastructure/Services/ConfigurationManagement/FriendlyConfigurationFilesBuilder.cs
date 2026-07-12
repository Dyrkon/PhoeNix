using PhoeNix.Application.Abstractions.Git;
using PhoeNix.Application.Models.Files;
using PhoeNix.Common.Utilities;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.ConfigurationManagement;

public class FriendlyConfigurationFilesBuilder(
    IGitOpsModuleFilesBuilder moduleFilesBuilder) : IGitOpsConfigurationFilesBuilder
{
    public Result<Folder> BuildConfigurationFiles(ConfigurationBuildResult configurationBuild)
    {
        var layout = new FriendlyConfigurationLayout();

        var systemPathsList = configurationBuild.Systems.Aggregate("", (current, s) =>
            current +
            $"{SlugGenerator.ToSlug(s.Name)} = import ./{layout.SystemPath(s.Name, s.Architecture.ToArchitectureString())} {{ inherit inputs sharedModules lib; }};\n");

        var sharedModulesList = configurationBuild.CommonModules.Aggregate("",
            (current, m) => current + $" ./{layout.SharedModulePath(m.Name)}");

        var checksPaths = string.Empty;

        foreach (var moduleBuildResult in configurationBuild.CommonModules.Where(m =>
                     m.ModuleTests != null && m.ModuleTests.Count != 0))
            if (moduleBuildResult.ModuleTests != null)
                foreach (var moduleTest in moduleBuildResult.ModuleTests)
                    checksPaths +=
                        $"{SlugGenerator.ToSlug(moduleTest.Name)} = import ./{layout.SharedModuleTestPath(moduleBuildResult.Name, moduleTest.Name)} {{ inherit inputs pkgs lib system; }}; ";

        foreach (var system in configurationBuild.Systems)
        foreach (var moduleBuildResult in system.Modules)
            if (moduleBuildResult.ModuleTests != null)
                foreach (var moduleTestBuildResult in moduleBuildResult.ModuleTests)
                    checksPaths +=
                        $"{SlugGenerator.ToSlug(system.Name)}-{SlugGenerator.ToSlug(moduleBuildResult.Name)}-{SlugGenerator.ToSlug(moduleTestBuildResult.Name)} = " +
                        $"import ./{layout.SystemModuleTestPath(system.Name, moduleBuildResult.Name, moduleTestBuildResult.Name)} {{ inherit inputs pkgs lib system; }}; ";

        var configSlug = SlugGenerator.ToSlug(configurationBuild.Title);

        var files = new List<FileBase>
        {
            new TextFile("flake.nix",
                configurationBuild.Content
                    .Replace(configurationBuild.SystemsPlaceholder, systemPathsList)
                    .Replace(configurationBuild.SharedModulesPlaceholder, sharedModulesList)
                    .Replace(configurationBuild.ChecksPlaceholder, checksPaths)),
            new Folder(layout.SharedModulesPath,
                configurationBuild.CommonModules.Select(moduleFilesBuilder.BuildModule).ToList()),
            new Folder(layout.SystemsPath,
                configurationBuild.Systems.Select(moduleFilesBuilder.BuildSystemModule).ToList())
        };

        return new Folder(configSlug, files);
    }
}
