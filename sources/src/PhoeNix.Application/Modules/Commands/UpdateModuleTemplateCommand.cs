using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Application.Repositories;
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
    IReadOnlyList<RequiredInputDefinitionModel> RequiredInputs) : ICommand<ModuleTemplateResponse>;

internal sealed class UpdateModuleTemplateHandler(
    IModuleTemplateRepository moduleTemplateRepository,
    ICurrentUserAccessor currentUserAccessor)
    : ICommandHandler<UpdateModuleTemplateCommand, ModuleTemplateResponse>
{
    public Task<Result<ModuleTemplateResponse>> Handle(
        UpdateModuleTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Task.FromResult(Result.Failure<ModuleTemplateResponse>(userIdResult.Error));

        var userId = userIdResult.Value;

        return moduleTemplateRepository
            .GetByIdAsync(request.ModuleTemplateId, cancellationToken)
            .EnsureNotNull(ModuleErrors.NotFound(request.ModuleTemplateId))
            .Ensure(t => t.OwnerId == userId, ModuleErrors.NotFound(request.ModuleTemplateId))
            .Bind(async template =>
            {
                var templateWithSameName = await moduleTemplateRepository.GetByNameAsync(request.Name, userId, cancellationToken);
                if (templateWithSameName is not null && templateWithSameName.Id != template.Id)
                    return Result.Failure<ModuleTemplateResponse>(ModuleErrors.NameAlreadyExists(request.Name));

                var editableValueTypes = request.EditableValueTypes
                    .Select(x => ModuleMappings.MapEntryValueDefinitionToDomain(template.Id, x))
                    .ToList();

                var tests = request.Tests
                    .Select(ModuleMappings.MapModuleTemplateTestToDomain)
                    .ToList();

                return Apply(template, request, editableValueTypes, tests).Map(ModuleMappings.MapModuleToDto);
            });
    }

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