using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Models.Modules;

namespace PhoeNix.Application.Mappings;

public static class ModuleMappings
{
    public static ModuleListResponse MapModuleToListDto(Module module)
    {
        return new ModuleListResponse(module.Id, module.Name, module.Enabled, module.Type);
    }

    public static ModuleResponse MapModuleToDto(Module module)
    {
        return new ModuleResponse(module.Id, module.Name, module.Enabled, module.Type, module.Content,
            module.EditableValues.Select(MapEntryValueToDto).ToList(), module.SupportedArchitectures.ToList());
    }

    public static EntryValueResponse MapEntryValueToDto(IEntryValue entryValue)
    {
        return new EntryValueResponse(entryValue.Id, entryValue.Name, entryValue.Placeholder, entryValue.Value);
    }
}