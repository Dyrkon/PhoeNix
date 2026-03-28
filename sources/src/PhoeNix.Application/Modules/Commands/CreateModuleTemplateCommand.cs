using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Modules.Commands;

public sealed record CreateModuleTemplateCommand(
    string Name,
    bool Enabled,
    ModuleType Type,
    string Content,
    IReadOnlyList<Architecture> SupportedArchitectures,
    IReadOnlyList<ModuleTemplateEntryValueDefinitionModel> EditableValueTypes,
    IReadOnlyList<ModuleTemplateTestUpsertModel> Tests) : ICommand<ModuleTemplateResponse>;

internal sealed class CreateModuleTemplateHandler(
    IModuleTemplateRepository moduleTemplateRepository)
    : ICommandHandler<CreateModuleTemplateCommand, ModuleTemplateResponse>
{
    public async Task<Result<ModuleTemplateResponse>> Handle(
        CreateModuleTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var existingTemplate = await moduleTemplateRepository.GetByNameAsync(request.Name, cancellationToken);

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
                request.Name,
                request.Enabled,
                request.Type,
                request.SupportedArchitectures)
            .Tap(template => template.ChangeContent(request.Content, editableValueTypes))
            .Tap(template => template.ReconcileTests(tests))
            .Tap(moduleTemplateRepository.Add)
            .Map(ModuleMappings.MapModuleToDto);
    }
}