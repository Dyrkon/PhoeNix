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

        var existing = await moduleTemplateRepository.GetByNameAsync(data.Name, userId, cancellationToken);

        if (existing is not null)
            return Result.Failure<ModuleTemplateResponse>(ModuleErrors.NameAlreadyExists(data.Name));

        var moduleTemplateId = new ModuleTemplateId(Guid.NewGuid());

        var editableValueTypes = data.EditableValueTypes
            .Select(e => new ModuleTemplateEntryValueDefinitionModel(
                e.Name, e.Placeholder, e.BindingKind, e.ValueKind,
                e.DefaultValue, e.DefaultLowerValue,
                e.IntegerMin, e.IntegerMax, e.DecimalMin, e.DecimalMax,
                e.AllowLowerValue, e.Options?.ToList(), e.BindingIndex))
            .Select(model => ModuleMappings.MapEntryValueDefinitionToDomain(moduleTemplateId, model))
            .ToList();

        var tests = data.Tests
            .Select(t => new ModuleTemplateTestDefinition(null, t.Name, t.Content, t.VariableNames.ToList()))
            .ToList();

        var requiredInputs = (data.RequiredInputs ?? [])
            .Select(r => (r.Name, r.Source));

        return ModuleTemplate.Create(
                moduleTemplateId,
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
}
