using Microsoft.Extensions.Logging;
using PhoeNix.Application.Abstractions.Setup;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public sealed class RuntimeBindingResolver(
    ILogger<RuntimeBindingResolver> logger) : IRuntimeBindingResolver
{
    public Result<Configuration> ApplyBindings(
        Configuration configuration,
        IEnumerable<ModuleTemplate> templates,
        SetupTarget target)
    {
        var templateMap = templates.ToDictionary(t => t.Id);

        logger.LogInformation(
            "Applying runtime bindings for machine {MachineId}. RankedDiskCount={RankedDiskCount}",
            target.MachineId.Value,
            target.RankedDiskAssignments.Count);

        foreach (var disk in target.RankedDiskAssignments.OrderBy(d => d.Index))
            logger.LogInformation(
                "Runtime binding source disk: index={Index}, path={DiskPath}",
                disk.Index,
                disk.DiskByIdPath);

        foreach (var module in configuration.Modules)
        {
            if (!templateMap.TryGetValue(module.ModuleTemplateId, out var template))
                return Result.Failure<Configuration>(new Error(
                    "ModuleTemplateNotFound",
                    $"Module template '{module.ModuleTemplateId}' was not found."));

            var sharedBindingResult = ApplyBindingsToModule(module, template, target, "shared");
            if (sharedBindingResult.IsFailure)
                return Result.Failure<Configuration>(sharedBindingResult.Error);
        }

        foreach (var system in configuration.SystemSpecifications)
        {
            logger.LogInformation(
                "Applying bindings to system {SystemId} / {SystemName}",
                system.Id.Value,
                system.Name);

            foreach (var module in system.Modules)
            {
                if (!templateMap.TryGetValue(module.ModuleTemplateId, out var template))
                    return Result.Failure<Configuration>(new Error(
                        "ModuleTemplateNotFound",
                        $"Module template '{module.ModuleTemplateId}' was not found."));

                var systemBindingResult = ApplyBindingsToModule(module, template, target, $"system:{system.Name}");
                if (systemBindingResult.IsFailure)
                    return Result.Failure<Configuration>(systemBindingResult.Error);
            }
        }

        return Result.Success(configuration);
    }

    private Result ApplyBindingsToModule(
        ModuleValue module,
        ModuleTemplate template,
        SetupTarget target,
        string scope)
    {
        logger.LogInformation(
            "Inspecting {Scope} module {TemplateName} ({TemplateId})",
            scope,
            template.Name,
            template.Id.Value);

        var newEntries = new List<EntryValue>();

        foreach (var entry in module.EditableValues)
        {
            var definition = template.EditableValueTypes.FirstOrDefault(d => d.Name == entry.Name);

            if (definition is null)
                return Result.Failure(new Error(
                    "EntryDefinitionNotFound",
                    $"Entry definition '{entry.Name}' was not found in template '{template.Name}'."));

            logger.LogInformation(
                "Entry {EntryName} in template {TemplateName}: bindingKind={BindingKind}, bindingIndex={BindingIndex}, currentValue={CurrentValue}",
                entry.Name,
                template.Name,
                definition.BindingKind,
                definition.BindingIndex,
                entry.Value);

            if (definition.BindingKind == EntryBindingKind.RankedDiskCandidate)
            {
                if (definition.BindingIndex is null)
                    return Result.Failure(new Error(
                        "BindingIndexMissing",
                        $"Entry '{entry.Name}' in template '{template.Name}' requires BindingIndex."));

                var disk = target.RankedDiskAssignments
                    .FirstOrDefault(d => d.Index == definition.BindingIndex.Value);

                if (disk is null)
                    return Result.Failure(new Error(
                        "DiskBindingOutOfRange",
                        $"No disk found for index {definition.BindingIndex.Value} in template '{template.Name}'."));

                logger.LogInformation(
                    "Applying ranked disk binding for entry {EntryName}: {OldValue} -> \"{DiskPath}\"",
                    entry.Name,
                    entry.Value,
                    disk.DiskByIdPath);

                entry.SetValue($"\"{disk.DiskByIdPath}\"");
            }

            logger.LogInformation(
                "Resolved entry {EntryName} in template {TemplateName} to value {ResolvedValue}",
                entry.Name,
                template.Name,
                entry.Value);

            newEntries.Add(entry);
        }

        var result = module.ChangeEntry(newEntries);
        if (result.IsFailure)
            return result.Error;

        return Result.Success();
    }
}