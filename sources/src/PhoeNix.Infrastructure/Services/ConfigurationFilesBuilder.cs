using PhoeNix.Application.Abstractions.Nix;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Models.Files;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public class ConfigurationFilesBuilder(
    IModuleFilesBuilder moduleFilesBuilder) : IConfigurationFilesBuilder
{
    public Result<Folder> BuildConfiguration(ConfigurationBuildResult configurationBuild)
    {
        var confLayout = new ConfigurationLayout();
        var systemPathsList = configurationBuild.Systems.Aggregate("", (current, s) =>
            current +
            $"{s.Id.ToStringWithPrefix()} = import ./{confLayout.SystemPath(s.Id, s.Architecture)} {{ inherit inputs sharedModules lib; }};\n");

        var sharedModulesList = configurationBuild.CommonModules.Aggregate("",
            (current, m) => current + $"./{confLayout.SharedModulePath(m.TemplateId)}");

        var checksPaths = string.Empty;

        foreach (var moduleBuildResult in configurationBuild.CommonModules.Where(m =>
                     m.ModuleTests != null && m.ModuleTests.Count != 0))
            if (moduleBuildResult.ModuleTests != null)
                foreach (var moduleTest in moduleBuildResult.ModuleTests)
                    checksPaths +=
                        $"{moduleTest.Id.ToStringWithPrefix()} = import ./{confLayout.SharedModuleTestPath(moduleBuildResult.TemplateId, moduleTest.Id)} {{ inherit inputs pkgs lib system; }}; ";

        foreach (var system in configurationBuild.Systems)
        foreach (var moduleBuildResult in system.Modules)
            if (moduleBuildResult.ModuleTests != null)
                foreach (var moduleTestBuildResult in moduleBuildResult.ModuleTests)
                    checksPaths +=
                        $"{moduleTestBuildResult.Id.ToStringWithPrefix()} = " +
                        $"import ./{confLayout.SystemModuleTestPath(system.Id, moduleBuildResult.TemplateId, moduleTestBuildResult.Id)} {{ inherit inputs pkgs lib system; }}; ";


        var files = new List<FileBase>
        {
            new TextFile("flake.nix",
                configurationBuild.Content
                    .Replace(configurationBuild.SystemsPlaceholder, systemPathsList)
                    .Replace(configurationBuild.SharedModulesPlaceholder, sharedModulesList)
                    .Replace(configurationBuild.ChecksPlaceholder, checksPaths)),
            new Folder(confLayout.SharedModulesPath,
                configurationBuild.CommonModules.Select(moduleFilesBuilder.BuildModule)),
            new Folder(confLayout.SystemsPath,
                configurationBuild.Systems.Select(moduleFilesBuilder.BuildSystemModule))
        };

        return new Folder(configurationBuild.Id, files);
    }
}