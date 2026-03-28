using PhoeNix.Application.Models.Modules;
using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Application.Mappings;

public static class ModuleMappings
{
    public static ModuleTemplateListResponse MapModuleToListDto(ModuleTemplate moduleTemplate)
    {
        return new ModuleTemplateListResponse(
            moduleTemplate.Id.Value,
            moduleTemplate.Name,
            moduleTemplate.Enabled,
            moduleTemplate.Type,
            moduleTemplate.SupportedArchitectures.ToList());
    }

    public static ModuleTemplateResponse MapModuleToDto(ModuleTemplate moduleTemplate)
    {
        return new ModuleTemplateResponse(
            moduleTemplate.Id.Value,
            moduleTemplate.Name,
            moduleTemplate.Enabled,
            moduleTemplate.Type,
            moduleTemplate.Content,
            moduleTemplate.RequiresSetupBindings,
            moduleTemplate.EditableValueTypes.Select(MapEntryValueDefinitionToDto).ToList(),
            moduleTemplate.SupportedArchitectures.ToList(),
            moduleTemplate.Tests.Select(MapModuleTestToDto).ToList());
    }

    public static EntryValueDefinitionResponse MapEntryValueDefinitionToDto(EntryValueDefinition entryValue)
    {
        return new EntryValueDefinitionResponse(
            entryValue.Name,
            entryValue.Placeholder,
            entryValue.InputType,
            entryValue.BindingKind,
            entryValue.BindingIndex);
    }

    public static ModuleTemplateTestResponse MapModuleTestToDto(Test test)
    {
        return new ModuleTemplateTestResponse(
            test.Id.Value,
            test.Name,
            test.Content,
            test.VariableNames.ToList());
    }

    public static EntryValueDefinition MapEntryValueDefinitionToDomain(
        ModuleTemplateId moduleTemplateId,
        ModuleTemplateEntryValueDefinitionModel model)
    {
        return new EntryValueDefinition(
            moduleTemplateId,
            model.Name,
            model.Placeholder,
            model.InputType,
            model.BindingKind,
            model.BindingIndex);
    }

    public static ModuleTemplateTestDefinition MapModuleTemplateTestToDomain(ModuleTemplateTestUpsertModel model)
    {
        return new ModuleTemplateTestDefinition(
            model.Id is null ? null : new TestId(model.Id.Value),
            model.Name,
            model.Content,
            model.VariableNames);
    }

    public static ModuleValueResponse MapModuleValueToDto(ModuleValue moduleValue)
    {
        return new ModuleValueResponse(
            moduleValue.Id.Value,
            moduleValue.Enabled,
            moduleValue.EditableValues.Select(MapEntryValueToDto).ToList());
    }

    public static EntryValueResponse MapEntryValueToDto(EntryValue entryValue)
    {
        return new EntryValueResponse(entryValue.Name, entryValue.Placeholder, entryValue.Value);
    }

    public static ModuleValueListResponse MapModuleValueToListDto(ModuleValue moduleValue)
    {
        return new ModuleValueListResponse(moduleValue.Id.Value, moduleValue.Enabled);
    }
}