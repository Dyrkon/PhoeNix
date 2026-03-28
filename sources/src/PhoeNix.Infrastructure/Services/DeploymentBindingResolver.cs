using Microsoft.Extensions.Logging;
using PhoeNix.Application.Abstractions.Deployment;
using PhoeNix.Application.Abstractions.Setup;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public sealed class DeploymentBindingResolver(
    ILogger<DeploymentBindingResolver> logger) : IDeploymentBindingResolver
{
    public Result<Configuration> ApplyBindings(
        Configuration configuration,
        IEnumerable<ModuleTemplate> templates,
        DeploymentSnapshot deploymentSnapshot)
    {
        var templateMap = templates.ToDictionary(t => t.Id);

        logger.LogInformation(
            "Applying deployment bindings. BoundDiskCount={BoundDiskCount}",
            deploymentSnapshot.BoundDisks.Count);

        foreach (var disk in deploymentSnapshot.BoundDisks.OrderBy(d => d.Index))
            logger.LogDebug(
                "Deployment binding source disk: index={Index}, path={DiskPath}",
                disk.Index,
                disk.StableDevicePath);

        foreach (var module in configuration.Modules.Where(m => m.Enabled))
        {
            if (!templateMap.TryGetValue(module.ModuleTemplateId, out var template))
                return Result.Failure<Configuration>(new Error(
                    "ModuleTemplateNotFound",
                    $"Module template '{module.ModuleTemplateId}' was not found."));

            var sharedBindingResult = ApplyBindingsToModule(module, template, deploymentSnapshot, "shared");
            if (sharedBindingResult.IsFailure)
            {
                logger.LogWarning(
                    "Deployment binding failed in template {TemplateName}: {ErrorCode} {ErrorDescription}",
                    template.Name,
                    sharedBindingResult.Error.Code,
                    sharedBindingResult.Error.Description);

                return Result.Failure<Configuration>(sharedBindingResult.Error);
            }
        }

        foreach (var system in configuration.SystemSpecifications)
        {
            logger.LogDebug(
                "Applying deployment bindings to system {SystemId} / {SystemName}",
                system.Id.Value,
                system.Name);

            foreach (var module in system.Modules.Where(m => m.Enabled))
            {
                if (!templateMap.TryGetValue(module.ModuleTemplateId, out var template))
                    return Result.Failure<Configuration>(new Error(
                        "ModuleTemplateNotFound",
                        $"Module template '{module.ModuleTemplateId}' was not found."));

                var systemBindingResult =
                    ApplyBindingsToModule(module, template, deploymentSnapshot, $"system:{system.Name}");
                if (systemBindingResult.IsFailure)
                {
                    logger.LogWarning(
                        "Deployment binding failed in template {TemplateName}: {ErrorCode} {ErrorDescription}",
                        template.Name,
                        systemBindingResult.Error.Code,
                        systemBindingResult.Error.Description);

                    return Result.Failure<Configuration>(systemBindingResult.Error);
                }
            }
        }

        logger.LogInformation("Deployment bindings applied.");

        return Result.Success(configuration);
    }

    private Result ApplyBindingsToModule(
        ModuleValue module,
        ModuleTemplate template,
        DeploymentSnapshot deploymentSnapshot,
        string scope)
    {
        logger.LogDebug(
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

            logger.LogDebug(
                "Entry {EntryName} in template {TemplateName}: bindingKind={BindingKind}, bindingIndex={BindingIndex}",
                entry.Name,
                template.Name,
                definition.BindingKind,
                definition.BindingIndex);

            if (definition.BindingKind == EntryBindingKind.RankedDiskCandidate)
            {
                if (definition.BindingIndex is null)
                    return Result.Failure(new Error(
                        "BindingIndexMissing",
                        $"Entry '{entry.Name}' in template '{template.Name}' requires BindingIndex."));

                var disk = deploymentSnapshot.BoundDisks
                    .FirstOrDefault(d => d.Index == definition.BindingIndex.Value);

                if (disk is null)
                    return Result.Failure(new Error(
                        "DiskBindingOutOfRange",
                        $"No deployment disk found for index {definition.BindingIndex.Value} in template '{template.Name}'."));

                logger.LogDebug(
                    "Applying deployment disk binding for entry {EntryName}: {DiskPath}",
                    entry.Name,
                    disk.StableDevicePath);

                entry.SetValue($"\"{disk.StableDevicePath}\"");
            }

            newEntries.Add(entry);
        }

        var result = module.ChangeEntry(newEntries);
        if (result.IsFailure)
            return result.Error;

        return Result.Success();
    }
}