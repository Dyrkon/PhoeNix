using System.Text.Json;
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
            moduleTemplate.Tests.Select(MapModuleTestToDto).ToList(),
            moduleTemplate.RequiredInputs.Select(MapRequiredInputToDto).ToList());
    }

    public static RequiredInputDefinitionResponse MapRequiredInputToDto(RequiredInputDefinition input)
    {
        return new RequiredInputDefinitionResponse(input.Name, input.Source);
    }

    public static EntryValueDefinitionResponse MapEntryValueDefinitionToDto(EntryValueDefinition entryValue)
    {
        return new EntryValueDefinitionResponse(
            entryValue.Name,
            entryValue.Placeholder,
            entryValue.BindingKind,
            entryValue.ValueKind,
            entryValue.DefaultValue,
            entryValue.DefaultLowerValue,
            entryValue.IntegerMin,
            entryValue.IntegerMax,
            entryValue.DecimalMin,
            entryValue.DecimalMax,
            entryValue.AllowLowerValue,
            entryValue.GetOptions(),
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
            model.BindingKind,
            model.ValueKind,
            model.DefaultValue,
            model.DefaultLowerValue,
            model.IntegerMin,
            model.IntegerMax,
            model.DecimalMin,
            model.DecimalMax,
            model.AllowLowerValue,
            model.Options is null ? null : JsonSerializer.Serialize(model.Options),
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
        return entryValue switch
        {
            TextValue textValue => new EntryValueResponse(
                textValue.Id.Value,
                textValue.Name,
                textValue.Placeholder,
                textValue.Kind,
                textValue.Value,
                null, null, null, null, null, null, null, null, null),

            IntegerRangeValue integerRangeValue => new EntryValueResponse(
                integerRangeValue.Id.Value,
                integerRangeValue.Name,
                integerRangeValue.Placeholder,
                integerRangeValue.Kind,
                integerRangeValue.Value,
                integerRangeValue.Min,
                integerRangeValue.Max,
                integerRangeValue.LowerValue,
                integerRangeValue.UpperValue,
                null, null, null, null, null),

            DecimalRangeValue decimalRangeValue => new EntryValueResponse(
                decimalRangeValue.Id.Value,
                decimalRangeValue.Name,
                decimalRangeValue.Placeholder,
                decimalRangeValue.Kind,
                decimalRangeValue.Value,
                null, null, null, null,
                decimalRangeValue.Min,
                decimalRangeValue.Max,
                decimalRangeValue.LowerValue,
                decimalRangeValue.UpperValue,
                null),

            SingleChoiceValue singleChoiceValue => new EntryValueResponse(
                singleChoiceValue.Id.Value,
                singleChoiceValue.Name,
                singleChoiceValue.Placeholder,
                singleChoiceValue.Kind,
                singleChoiceValue.Value,
                null, null, null, null, null, null, null, null,
                singleChoiceValue.Options.ToList()),

            ListValue listValue => new EntryValueResponse(
                listValue.Id.Value,
                listValue.Name,
                listValue.Placeholder,
                listValue.Kind,
                listValue.Value,
                null, null, null, null, null, null, null, null, null,
                listValue.GetItems().ToList()),

            _ => throw new InvalidOperationException($"Unsupported entry value type '{entryValue.GetType().Name}'.")
        };
    }

    public static ModuleValueListResponse MapModuleValueToListDto(ModuleValue moduleValue)
    {
        return new ModuleValueListResponse(moduleValue.Id.Value, moduleValue.Enabled);
    }
}