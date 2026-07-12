using PhoeNix.Application.Abstractions.Git;
using PhoeNix.Application.Models.Files;
using PhoeNix.Common.Utilities;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.ConfigurationManagement;

public class FriendlyModuleFilesBuilder : IGitOpsModuleFilesBuilder
{
    private readonly FriendlyConfigurationLayout _layout = new();

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
                files.Add(new TextFile($"{SlugGenerator.ToSlug(moduleTest.Name)}.nix", moduleTest.Content
                    .Replace(moduleTest.InputsLocationPlaceholder, inputsLocationFolder)
                    .Replace(moduleTest.TestedModulePathPlaceholder, $"./{DefaultNames.ModuleName}.nix")));

        return new Folder(SlugGenerator.ToSlug(moduleBuild.Name), files);
    }

    public Folder BuildSystemModule(SystemBuildResult systemBuild)
    {
        var systemSlug = SlugGenerator.ToSlug(systemBuild.Name);

        var moduleList = string.Empty;

        foreach (var module in systemBuild.Modules)
        {
            var moduleSlug = SlugGenerator.ToSlug(module.Name);
            moduleList += $"./Modules/{moduleSlug}/{DefaultNames.ModuleName}.nix ";
        }

        var files = new List<FileBase>
        {
            new Folder("Modules", systemBuild.Modules.Select(BuildModule)),
            new TextFile(
                $"{systemBuild.Architecture.ToArchitectureString()}.nix",
                systemBuild.Content.Replace(systemBuild.ModulesListPlaceholder, moduleList))
        };

        return new Folder(systemSlug, files);
    }
}
