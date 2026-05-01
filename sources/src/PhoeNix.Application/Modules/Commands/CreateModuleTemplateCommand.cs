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

public sealed record CreateModuleTemplateCommand(
    string Name,
    bool Enabled,
    ModuleType Type,
    string Content,
    IReadOnlyList<Architecture> SupportedArchitectures,
    IReadOnlyList<ModuleTemplateEntryValueDefinitionModel> EditableValueTypes,
    IReadOnlyList<ModuleTemplateTestUpsertModel> Tests,
    IReadOnlyList<RequiredInputDefinitionModel> RequiredInputs) : ICommand<ModuleTemplateResponse>;

internal sealed class CreateModuleTemplateHandler(
    IModuleTemplateRepository moduleTemplateRepository,
    ICurrentUserAccessor currentUserAccessor)
    : ICommandHandler<CreateModuleTemplateCommand, ModuleTemplateResponse>
{
    public async Task<Result<ModuleTemplateResponse>> Handle(
        CreateModuleTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<ModuleTemplateResponse>(userIdResult.Error);

        var userId = userIdResult.Value;

        var existingTemplate = await moduleTemplateRepository.GetByNameAsync(request.Name, userId, cancellationToken);

        if (existingTemplate is not null)
            return Result.Failure<ModuleTemplateResponse>(ModuleErrors.NameAlreadyExists(request.Name));

        var moduleTemplateId = new ModuleTemplateId(Guid.NewGuid());

        var editableValueTypes = request.EditableValueTypes
            .Select(x => ModuleMappings.MapEntryValueDefinitionToDomain(moduleTemplateId, x))
            .ToList();

        var tests = request.Tests
            .Select(ModuleMappings.MapModuleTemplateTestToDomain)
            .ToList();

        return ModuleTemplate.Create(
                moduleTemplateId,
                userId,
                request.Name,
                request.Enabled,
                request.Type,
                request.SupportedArchitectures)
            .Tap(template => template.ChangeContent(request.Content, editableValueTypes))
            .Tap(template => template.ReconcileTests(tests))
            .Tap(template => template.SetRequiredInputs(request.RequiredInputs.Select(r => (r.Name, r.Source))))
            .Tap(moduleTemplateRepository.Add)
            .Map(ModuleMappings.MapModuleToDto);
    }
}