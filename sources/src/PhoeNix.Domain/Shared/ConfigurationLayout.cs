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

    public string SharedModuleFolderPath(ModuleTemplateId moduleTemplateId)
    {
        return $"{SharedModulesPath}/{moduleTemplateId.ToStringWithPrefix()}";
    }

    public string SharedModulePath(ModuleTemplateId moduleTemplateId, string? moduleName = null)
    {
        moduleName ??= DefaultNames.ModuleName;
        return $"{SharedModuleFolderPath(moduleTemplateId)}/{moduleName}.nix";
    }

    public string SharedModuleTestPath(ModuleTemplateId moduleTemplateId, TestId testId)
    {
        return $"{SharedModuleFolderPath(moduleTemplateId)}/{testId.ToStringWithPrefix()}.nix";
    }

    public string SystemModulesFolderPath(SystemId systemId)
    {
        return $"{SystemsPath}/{systemId.ToStringWithPrefix()}/Modules";
    }

    public string SystemModuleFolderPath(SystemId systemId, ModuleTemplateId moduleTemplateId)
    {
        return $"{SystemModulesFolderPath(systemId)}/{moduleTemplateId.ToStringWithPrefix()}";
    }

    public string SystemModulePath(SystemId systemId, ModuleTemplateId moduleTemplateId, string? moduleName = null)
    {
        moduleName ??= DefaultNames.ModuleName;
        return $"{SystemModuleFolderPath(systemId, moduleTemplateId)}/{moduleName}.nix";
    }

    public string SystemModuleTestPath(SystemId systemId, ModuleTemplateId moduleTemplateId, TestId testId)
    {
        return $"{SystemModuleFolderPath(systemId, moduleTemplateId)}/{testId.ToStringWithPrefix()}.nix";
    }
}