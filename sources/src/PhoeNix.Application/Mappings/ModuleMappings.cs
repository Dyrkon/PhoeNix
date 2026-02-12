using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Models.Modules;

namespace PhoeNix.Application.Mappings;

public static class ModuleMappings
{
    public static ModuleListResponse MapModuleToListDto(ModuleTemplate moduleTemplate)
    {
        return new ModuleListResponse(moduleTemplate.Id, moduleTemplate.Name, moduleTemplate.Enabled,
            moduleTemplate.Type);
    }

    public static ModuleResponse MapModuleToDto(ModuleTemplate moduleTemplate)
    {
        return new ModuleResponse(moduleTemplate.Id, moduleTemplate.Name, moduleTemplate.Enabled, moduleTemplate.Type,
            moduleTemplate.Content,
            moduleTemplate.EditableValues.Select(MapEntryValueToDto).ToList(),
            moduleTemplate.SupportedArchitectures.ToList());
    }

    public static EntryValueResponse MapEntryValueToDto(EntryValue entryValue)
    {
        return new EntryValueResponse(entryValue.Id, entryValue.Name, entryValue.Placeholder, entryValue.Value);
    }
}