using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Application.Modules.Factories;
using PhoeNix.Application.Repositories;
using PhoeNix.Contracts.Modules;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Modules.Commands;

public sealed record UpdateModuleTemplateCommand(
    ModuleTemplateId ModuleTemplateId,
    string Name,
    bool Enabled,
    ModuleType Type,
    string Content,
    IReadOnlyList<Architecture> SupportedArchitectures,
    IReadOnlyList<ModuleTemplateEntryValueDefinitionModel> EditableValueTypes,
    IReadOnlyList<ModuleTemplateTestUpsertModel> Tests,
    IReadOnlyList<RequiredInputDefinitionModel> RequiredInputs) : ICommand<UpdateModuleTemplateResult>;

internal sealed class UpdateModuleTemplateHandler(
    IModuleTemplateRepository moduleTemplateRepository,
    IConfigurationRepository configurationRepository,
    ICurrentUserAccessor currentUserAccessor)
    : ICommandHandler<UpdateModuleTemplateCommand, UpdateModuleTemplateResult>
{
    public Task<Result<UpdateModuleTemplateResult>> Handle(
        UpdateModuleTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Task.FromResult(Result.Failure<UpdateModuleTemplateResult>(userIdResult.Error));

        var userId = userIdResult.Value;

        return moduleTemplateRepository
            .GetByIdAsync(request.ModuleTemplateId, cancellationToken)
            .EnsureNotNull(ModuleErrors.NotFound(request.ModuleTemplateId))
            .Ensure(t => t.OwnerId == userId, ModuleErrors.NotFound(request.ModuleTemplateId))
            .Bind(async template =>
            {
                var templateWithSameName = await moduleTemplateRepository.GetByNameAsync(request.Name, userId, cancellationToken);
                if (templateWithSameName is not null && templateWithSameName.Id != template.Id)
                    return Result.Failure<UpdateModuleTemplateResult>(ModuleErrors.NameAlreadyExists(request.Name));

                var editableValueTypes = request.EditableValueTypes
                    .Select(x => ModuleMappings.MapEntryValueDefinitionToDomain(template.Id, x))
                    .ToList();

                var tests = request.Tests
                    .Select(ModuleMappings.MapModuleTemplateTestToDomain)
                    .ToList();

                var applyResult = Apply(template, request, editableValueTypes, tests);
                if (applyResult.IsFailure)
                    return Result.Failure<UpdateModuleTemplateResult>(applyResult.Error);

                var syncResult = await SyncConfigurationModulesAsync(
                    request.ModuleTemplateId,
                    template.EditableValueTypes,
                    cancellationToken);

                if (syncResult.IsFailure)
                    return Result.Failure<UpdateModuleTemplateResult>(syncResult.Error);

                return Result.Success(new UpdateModuleTemplateResult(
                    ModuleMappings.MapModuleToDto(template),
                    syncResult.Value));
            });
    }

    private async Task<Result<IReadOnlyList<AffectedConfigurationSummary>>> SyncConfigurationModulesAsync(
        ModuleTemplateId moduleTemplateId,
        IReadOnlyList<EntryValueDefinition> updatedDefinitions,
        CancellationToken cancellationToken)
    {
        var configurations = await configurationRepository
            .GetAllUsingModuleTemplateAsync(moduleTemplateId, cancellationToken);

        var affected = new List<AffectedConfigurationSummary>();

        foreach (var configuration in configurations)
        {
            var changed = false;

            foreach (var moduleValue in GetModuleValuesForTemplate(configuration, moduleTemplateId))
            {
                var existingPlaceholders = moduleValue.EditableValues
                    .Select(e => e.Placeholder).ToHashSet();

                var newDefinitions = updatedDefinitions
                    .Where(d => !existingPlaceholders.Contains(d.Placeholder))
                    .ToList();

                var hasOrphans = moduleValue.EditableValues
                    .Any(e => updatedDefinitions.All(d => d.Placeholder != e.Placeholder));

                if (newDefinitions.Count == 0 && !hasOrphans)
                    continue;

                var newEntriesResult = ModuleEntryFactory.CreateDefaultEntries(moduleValue, newDefinitions);
                if (newEntriesResult.IsFailure)
                    return Result.Failure<IReadOnlyList<AffectedConfigurationSummary>>(newEntriesResult.Error);

                moduleValue.SyncEntries(updatedDefinitions, newEntriesResult.Value.ToList());
                changed = true;
            }

            if (changed)
                affected.Add(new AffectedConfigurationSummary(configuration.Id.Value, configuration.Title));
        }

        return Result.Success<IReadOnlyList<AffectedConfigurationSummary>>(affected);
    }

    private static IEnumerable<ModuleValue> GetModuleValuesForTemplate(
        Configuration configuration,
        ModuleTemplateId templateId) =>
        configuration.Modules.Where(m => m.ModuleTemplateId == templateId)
            .Concat(configuration.SystemSpecifications
                .SelectMany(s => s.Modules.Where(m => m.ModuleTemplateId == templateId)));

    private static Result<ModuleTemplate> Apply(
        ModuleTemplate template,
        UpdateModuleTemplateCommand request,
        IReadOnlyCollection<EntryValueDefinition> editableValueTypes,
        IReadOnlyCollection<ModuleTemplateTestDefinition> tests)
    {
        return Result.Success(template)
            .Tap(t => t.EditModule(request.Name))
            .Tap(t => t.SetEnabled(request.Enabled))
            .Tap(t => t.ChangeType(request.Type))
            .Tap(t => t.ReplaceArchitectureSupport(request.SupportedArchitectures))
            .Tap(t => t.ChangeContent(request.Content, editableValueTypes))
            .Tap(t => t.ReconcileTests(tests))
            .Tap(t => t.SetRequiredInputs(request.RequiredInputs.Select(r => (r.Name, r.Source))));
    }
}
