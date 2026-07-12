using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Repositories;
using PhoeNix.Contracts.Modules;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Modules.Commands;

public sealed record ImportModuleTemplateCommand(ModuleTemplateResponse ImportData) : ICommand<ModuleTemplateResponse>;

internal sealed class ImportModuleTemplateHandler(
    IModuleTemplateRepository moduleTemplateRepository,
    ICurrentUserAccessor currentUserAccessor)
    : ICommandHandler<ImportModuleTemplateCommand, ModuleTemplateResponse>
{
    public async Task<Result<ModuleTemplateResponse>> Handle(
        ImportModuleTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<ModuleTemplateResponse>(userIdResult.Error);

        var userId = userIdResult.Value;
        var data = request.ImportData;

        var importId = new ModuleTemplateId(data.Id);

        // Check if an entity with this ID already exists (upsert path)
        var existingById = await moduleTemplateRepository.GetByIdAsync(importId, cancellationToken);
        if (existingById is not null)
            return await UpdateExisting(existingById, data);

        // Check for name conflict
        var existingByName = await moduleTemplateRepository.GetByNameAsync(data.Name, userId, cancellationToken);
        if (existingByName is not null)
            return Result.Failure<ModuleTemplateResponse>(ModuleErrors.NameAlreadyExists(data.Name));

        return CreateNew(importId, userId, data);
    }

    private Task<Result<ModuleTemplateResponse>> UpdateExisting(ModuleTemplate existing, ModuleTemplateResponse data)
    {
        var editableValueTypes = BuildEditableValueTypes(existing.Id, data);
        var tests = BuildTests(data);
        var requiredInputs = (data.RequiredInputs ?? []).Select(r => (r.Name, r.Source));

        existing.EditModule(data.Name);
        existing.SetEnabled(data.Enabled);
        existing.ChangeType(data.Type);
        existing.ReplaceArchitectureSupport(data.SupportedArchitectures);
        existing.ChangeContent(data.Content, editableValueTypes);
        existing.ReconcileTests(tests);
        existing.SetRequiredInputs(requiredInputs);

        return Task.FromResult(Result.Success(ModuleMappings.MapModuleToDto(existing)));
    }

    private Result<ModuleTemplateResponse> CreateNew(ModuleTemplateId id, Domain.Entities.Users.UserId userId, ModuleTemplateResponse data)
    {
        var editableValueTypes = BuildEditableValueTypes(id, data);
        var tests = BuildTests(data);
        var requiredInputs = (data.RequiredInputs ?? []).Select(r => (r.Name, r.Source));

        return ModuleTemplate.Create(
                id,
                userId,
                data.Name,
                data.Enabled,
                data.Type,
                data.SupportedArchitectures)
            .Tap(template => template.ChangeContent(data.Content, editableValueTypes))
            .Tap(template => template.ReconcileTests(tests))
            .Tap(template => template.SetRequiredInputs(requiredInputs))
            .Tap(moduleTemplateRepository.Add)
            .Map(ModuleMappings.MapModuleToDto);
    }

    private static List<EntryValueDefinition> BuildEditableValueTypes(ModuleTemplateId id, ModuleTemplateResponse data)
    {
        return data.EditableValueTypes
            .Select(e => new ModuleTemplateEntryValueDefinitionModel(
                e.Name, e.Placeholder, e.BindingKind, e.ValueKind,
                e.DefaultValue, e.DefaultLowerValue,
                e.IntegerMin, e.IntegerMax, e.DecimalMin, e.DecimalMax,
                e.AllowLowerValue, e.Options?.ToList(), e.BindingIndex))
            .Select(model => ModuleMappings.MapEntryValueDefinitionToDomain(id, model))
            .ToList();
    }

    private static List<ModuleTemplateTestDefinition> BuildTests(ModuleTemplateResponse data)
    {
        return data.Tests
            .Select(t => new ModuleTemplateTestDefinition(null, t.Name, t.Content, t.VariableNames.ToList()))
            .ToList();
    }
}
