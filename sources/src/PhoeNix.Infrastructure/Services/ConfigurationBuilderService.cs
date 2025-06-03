using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Models.Files;
using PhoeNix.Domain.Services;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public class ConfigurationBuilderService(IModuleBuilderService moduleBuilderService) : IConfigurationBuilderService
{
    public Result<Folder> BuildConfiguration(ConfigurationBuildResult configurationBuild)
    {
        var sharedModulesList = string.Empty;
        var systemPathsList = string.Empty;
        var configModulesFolderPath = $"{configurationBuild.Title}Modules";
        var configSystemsFolderPath = $"{configurationBuild.Title}Systems";

        systemPathsList = configurationBuild.Systems.Aggregate(systemPathsList,
            (current, s) =>
                current +
                $"{s.Name} = import ./{configSystemsFolderPath}/{s.Name}/{s.Name}-{s.Architecture}.nix {{ inherit inputs sharedModules; }};\n");

        foreach (var m in configurationBuild.CommonModules)
            sharedModulesList += $"./{configModulesFolderPath}/{m.Name}/default.nix ";

        var files = new List<FileBase>
        {
            new TextFile("flake.nix",
                configurationBuild.Content
                    .Replace(configurationBuild.SystemsPlaceholder, systemPathsList)
                    .Replace(configurationBuild.SharedModulesPlaceholder, sharedModulesList)),
            new Folder(configModulesFolderPath,
                configurationBuild.CommonModules.Select(moduleBuilderService.BuildModule)),
            new Folder(configSystemsFolderPath,
                configurationBuild.Systems.Select(moduleBuilderService.BuildSystemModule))
        };

        return new Folder(configurationBuild.Title, files);
    }
}