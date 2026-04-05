using PhoeNix.Application.Models.Configurations;
using PhoeNix.Application.Models.Systems;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Application.Mappings;

public static class ConfigurationMappings
{
    public static ConfigurationResponse MapConfigurationToDto(
        Configuration configuration,
        IReadOnlyDictionary<ModuleTemplateId, ModuleTemplate> templatesById)
    {
        return new ConfigurationResponse(
            configuration.Id.Value,
            configuration.Title,
            configuration.Description,
            configuration.Inputs.Select(InputMappings.MapInputToDto).ToList(),
            configuration.Modules
                .Select(module => MapConfiguredModuleToDto(module, templatesById))
                .ToList(),
            configuration.SystemSpecifications
                .Select(system => MapConfiguredSystemToDto(system, templatesById))
                .ToList(),
            configuration.SupportedSystemArchitectures().ToList());
    }

    public static ConfigurationListResponse MapConfigurationToListDto(Configuration configuration)
    {
        return new ConfigurationListResponse(
            configuration.Id.Value,
            configuration.Title,
            configuration.Description);
    }

    private static ConfiguredSystemResponse MapConfiguredSystemToDto(
        Domain.Entities.Systems.System system,
        IReadOnlyDictionary<ModuleTemplateId, ModuleTemplate> templatesById)
    {
        return new ConfiguredSystemResponse(
            system.Id.Value,
            system.Name,
            system.Architecture,
            system.Modules
                .Select(module => MapConfiguredModuleToDto(module, templatesById))
                .ToList());
    }

    private static ConfiguredModuleResponse MapConfiguredModuleToDto(
        ModuleValue moduleValue,
        IReadOnlyDictionary<ModuleTemplateId, ModuleTemplate> templatesById)
    {
        if (!templatesById.TryGetValue(moduleValue.ModuleTemplateId, out var template))
            throw new InvalidOperationException(
                $"Module template '{moduleValue.ModuleTemplateId.Value}' was not provided for module value '{moduleValue.Id.Value}'.");

        return new ConfiguredModuleResponse(
            moduleValue.Id.Value,
            moduleValue.ModuleTemplateId.Value,
            template.Name,
            template.Enabled,
            moduleValue.Enabled,
            template.Type,
            template.RequiresSetupBindings,
            template.SupportedArchitectures.ToList(),
            moduleValue.EditableValues
                .Select(MapConfiguredModuleEntryToDto)
                .ToList());
    }

    private static ConfiguredModuleEntryResponse MapConfiguredModuleEntryToDto(EntryValue entryValue)
    {
        return entryValue switch
        {
            TextValue textValue => new ConfiguredModuleEntryResponse(
                textValue.Id.Value,
                textValue.Name,
                textValue.Placeholder,
                textValue.Kind,
                textValue.Value,
                null, null, null, null, null),

            IntegerRangeValue integerRangeValue => new ConfiguredModuleEntryResponse(
                integerRangeValue.Id.Value,
                integerRangeValue.Name,
                integerRangeValue.Placeholder,
                integerRangeValue.Kind,
                null,
                integerRangeValue.LowerValue,
                integerRangeValue.UpperValue,
                null, null, null),

            DecimalRangeValue decimalRangeValue => new ConfiguredModuleEntryResponse(
                decimalRangeValue.Id.Value,
                decimalRangeValue.Name,
                decimalRangeValue.Placeholder,
                decimalRangeValue.Kind,
                null, null, null,
                decimalRangeValue.LowerValue,
                decimalRangeValue.UpperValue,
                null),

            SingleChoiceValue singleChoiceValue => new ConfiguredModuleEntryResponse(
                singleChoiceValue.Id.Value,
                singleChoiceValue.Name,
                singleChoiceValue.Placeholder,
                singleChoiceValue.Kind,
                singleChoiceValue.Value,
                null, null, null, null,
                singleChoiceValue.Options.ToList()),

            _ => throw new InvalidOperationException(
                $"Unsupported entry value type '{entryValue.GetType().Name}'.")
        };
    }
}