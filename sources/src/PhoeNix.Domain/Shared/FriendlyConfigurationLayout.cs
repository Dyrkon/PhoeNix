using PhoeNix.Common.Utilities;

namespace PhoeNix.Domain.Shared;

public class FriendlyConfigurationLayout
{
    public string SharedModulesPath => "Modules";
    public string SystemsPath => "Systems";

    public string SystemPath(string systemName, string architectureString)
    {
        return $"{SystemsPath}/{SlugGenerator.ToSlug(systemName)}/{architectureString}.nix";
    }

    public string SystemFolderPath(string systemName)
    {
        return $"{SystemsPath}/{SlugGenerator.ToSlug(systemName)}";
    }

    public string SharedModuleFolderPath(string moduleName)
    {
        return $"{SharedModulesPath}/{SlugGenerator.ToSlug(moduleName)}";
    }

    public string SharedModulePath(string moduleName)
    {
        return $"{SharedModuleFolderPath(moduleName)}/{DefaultNames.ModuleName}.nix";
    }

    public string SharedModuleTestPath(string moduleName, string testName)
    {
        return $"{SharedModuleFolderPath(moduleName)}/{SlugGenerator.ToSlug(testName)}.nix";
    }

    public string SystemModulesFolderPath(string systemName)
    {
        return $"{SystemFolderPath(systemName)}/Modules";
    }

    public string SystemModuleFolderPath(string systemName, string moduleName)
    {
        return $"{SystemModulesFolderPath(systemName)}/{SlugGenerator.ToSlug(moduleName)}";
    }

    public string SystemModulePath(string systemName, string moduleName)
    {
        return $"{SystemModuleFolderPath(systemName, moduleName)}/{DefaultNames.ModuleName}.nix";
    }

    public string SystemModuleTestPath(string systemName, string moduleName, string testName)
    {
        return $"{SystemModuleFolderPath(systemName, moduleName)}/{SlugGenerator.ToSlug(testName)}.nix";
    }
}
