using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Modules.Factories;
using PhoeNix.Application.Repositories;
using PhoeNix.Contracts.Configurations;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public sealed record ImportConfigurationCommand(ConfigurationResponse ImportData) : ICommand<ConfigurationResponse>;

internal sealed class ImportConfigurationHandler(
    IConfigurationRepository configurationRepository,
    IModuleTemplateRepository moduleTemplateRepository,
    ICurrentUserAccessor currentUserAccessor)
    : ICommandHandler<ImportConfigurationCommand, ConfigurationResponse>
{
    public async Task<Result<ConfigurationResponse>> Handle(
        ImportConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<ConfigurationResponse>(userIdResult.Error);

        var data = request.ImportData;

        var configResult = Configuration.Create(
            new ConfigurationId(Guid.NewGuid()),
            userIdResult.Value,
            data.Title,
            data.Description);

        if (configResult.IsFailure)
            return Result.Failure<ConfigurationResponse>(configResult.Error);

        var configuration = configResult.Value;

        foreach (var input in data.Inputs)
        {
            var addResult = configuration.AddInput(input.Source, input.Name);

            if (addResult.IsSuccess)
            {
                var follows = input.Followers
                    .Select(f => new InputFollowDraft(f.FollowName, f.FollowValue))
                    .ToList();
                addResult.Value.ReplaceFollows(follows);
            }
        }

        var allTemplates = (await moduleTemplateRepository.GetAllAsync(userIdResult.Value, cancellationToken)).ToList();
        var templatesById = allTemplates.ToDictionary(t => t.Id);
        var templatesByName = allTemplates
            .GroupBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var module in data.Modules)
        {
            var template = ResolveTemplate(module.ModuleTemplateId, module.TemplateName, templatesById,
                templatesByName);
            if (template is null)
                continue;

            var addResult = configuration.AddModule(template.Id, module.Enabled);
            if (addResult.IsFailure)
                continue;

            var moduleValue = addResult.Value;
            var entriesResult = ModuleEntryFactory.CreateEntries(moduleValue, template.EditableValueTypes,
                module.Entries.Select(m =>
                        new ModuleEntryValueUpsertModel(m.Name, m.Placeholder, m.Kind, m.Value, m.IntegerUpperValue,
                            m.IntegerLowerValue, m.DecimalUpperValue, m.DecimalLowerValue, m.Value, m.ListItems))
                    .ToList());
            if (entriesResult.IsSuccess)
                moduleValue.ReplaceEntries(entriesResult.Value);
        }

        foreach (var system in data.Systems)
        {
            var addSystemResult = configuration.AddSystem(
                new SystemId(Guid.NewGuid()),
                system.Architecture,
                system.Name);

            if (addSystemResult.IsFailure)
                continue;

            var addedSystem = addSystemResult.Value;

            foreach (var module in system.Modules)
            {
                var template = ResolveTemplate(module.ModuleTemplateId, module.TemplateName, templatesById,
                    templatesByName);
                if (template is null)
                    continue;

                var addModuleResult = configuration.AddSystemModule(
                    addedSystem.Id,
                    template.Id,
                    template.SupportedArchitectures.ToList(),
                    module.Enabled);

                if (addModuleResult.IsFailure)
                    continue;

                var moduleValue = addModuleResult.Value;
                var entriesResult = ModuleEntryFactory.CreateDefaultEntries(moduleValue, template.EditableValueTypes);
                if (entriesResult.IsSuccess)
                    moduleValue.ReplaceEntries(entriesResult.Value);
            }
        }

        configurationRepository.Add(configuration);

        return Result.Success(ConfigurationMappings.MapConfigurationToDto(configuration, templatesById));
    }

    private static ModuleTemplate? ResolveTemplate(
        Guid moduleTemplateId,
        string templateName,
        Dictionary<ModuleTemplateId, ModuleTemplate> templatesById,
        Dictionary<string, ModuleTemplate> templatesByName)
    {
        var id = new ModuleTemplateId(moduleTemplateId);

        if (templatesById.TryGetValue(id, out var template))
            return template;

        templatesByName.TryGetValue(templateName, out var byName);
        return byName;
    }
}