using PhoeNix.Application.Models.Modules;
using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Application.Mappings;

public static class ModuleMappings
{
    public static ModuleTemplateListResponse MapModuleToListDto(ModuleTemplate moduleTemplate)
    {
        return new ModuleTemplateListResponse(moduleTemplate.Id, moduleTemplate.Name, moduleTemplate.Type);
    }

    public static ModuleTemplateResponse MapModuleToDto(ModuleTemplate moduleTemplate)
    {
        return new ModuleTemplateResponse(
            moduleTemplate.Id,
            moduleTemplate.Name,
            moduleTemplate.Type,
            moduleTemplate.Content,
            moduleTemplate.EditableValueTypes.Select(MapEntryValueDefinitionToDto).ToList(),
            moduleTemplate.SupportedArchitectures.ToList());
    }

    public static EntryValueDefinitionResponse MapEntryValueDefinitionToDto(EntryValueDefinition entryValue)
    {
        return new EntryValueDefinitionResponse(entryValue.Name, entryValue.Placeholder, entryValue.InputType);
    }

    public static ModuleValueResponse MapModuleValueToDto(ModuleValue moduleValue)
    {
        return new ModuleValueResponse(
            moduleValue.Id,
            moduleValue.Enabled,
            moduleValue.EditableValues.Select(MapEntryValueToDto).ToList());
    }

    public static EntryValueResponse MapEntryValueToDto(EntryValue entryValue)
    {
        return new EntryValueResponse(entryValue.Name, entryValue.Placeholder, entryValue.Value);
    }

    public static ModuleValueListResponse MapModuleValueToListDto(ModuleValue moduleValue)
    {
        return new ModuleValueListResponse(moduleValue.Id, moduleValue.Enabled);
    }
}