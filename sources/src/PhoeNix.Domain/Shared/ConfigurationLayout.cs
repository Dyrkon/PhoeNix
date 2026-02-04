using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Shared;

public class ConfigurationLayout
{
    public string SharedModulesPath => "Modules";
    public string SystemsPath => "Systems";

    public string SystemPath(SystemId systemId, Architecture architecture)
    {
        return $"{SystemsPath}/{systemId.ToStringWithPrefix()}/{architecture.ToArchitectureString()}.nix";
    }

    public string SystemName(SystemId id, Architecture architecture)
    {
        return $"{architecture.ToArchitectureString()}.nix";
    }

    public string SystemPath(SystemId id)
    {
        return $"{SystemsPath}/{id.ToStringWithPrefix()}";
    }

    public string SharedModuleFolderPath(ModuleId moduleId)
    {
        return $"{SharedModulesPath}/{moduleId.ToStringWithPrefix()}";
    }

    public string SharedModulePath(ModuleId moduleId, string? moduleName = null)
    {
        moduleName ??= DefaultNames.ModuleName;
        return $"{SharedModuleFolderPath(moduleId)}/{moduleName}.nix";
    }

    public string SharedModuleTestPath(ModuleId moduleId, TestId testId)
    {
        return $"{SharedModuleFolderPath(moduleId)}/{testId.ToStringWithPrefix()}.nix";
    }

    public string SystemModulesFolderPath(SystemId systemId)
    {
        return $"{SystemsPath}/{systemId.ToStringWithPrefix()}/Modules";
    }

    public string SystemModuleFolderPath(SystemId systemId, ModuleId moduleId)
    {
        return $"{SystemModulesFolderPath(systemId)}/{moduleId.ToStringWithPrefix()}";
    }

    public string SystemModulePath(SystemId systemId, ModuleId moduleId, string? moduleName = null)
    {
        moduleName ??= DefaultNames.ModuleName;
        return $"{SystemModuleFolderPath(systemId, moduleId)}/{moduleName}.nix";
    }

    public string SystemModuleTestPath(SystemId systemId, ModuleId moduleId, TestId testId)
    {
        return $"{SystemModuleFolderPath(systemId, moduleId)}/{testId.ToStringWithPrefix()}.nix";
    }
}