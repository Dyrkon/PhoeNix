using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
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
    IReadOnlyList<ModuleTemplateTestUpsertModel> Tests) : ICommand<ModuleTemplateResponse>;

internal sealed class UpdateModuleTemplateHandler(
    IModuleTemplateRepository moduleTemplateRepository)
    : ICommandHandler<UpdateModuleTemplateCommand, ModuleTemplateResponse>
{
    public async Task<Result<ModuleTemplateResponse>> Handle(
        UpdateModuleTemplateCommand request,
        CancellationToken cancellationToken)
    {
        var template = await moduleTemplateRepository.GetByIdAsync(
            request.ModuleTemplateId,
            cancellationToken);

        if (template is null)
            return Result.Failure<ModuleTemplateResponse>(ModuleErrors.NotFound(request.ModuleTemplateId));

        var templateWithSameName = await moduleTemplateRepository.GetByNameAsync(request.Name, cancellationToken);

        if (templateWithSameName is not null && templateWithSameName.Id != template.Id)
            return Result.Failure<ModuleTemplateResponse>(ModuleErrors.NameAlreadyExists(request.Name));

        var editableValueTypes = request.EditableValueTypes
            .Select(x => ModuleMappings.MapEntryValueDefinitionToDomain(template.Id, x))
            .ToList();

        var tests = request.Tests
            .Select(ModuleMappings.MapModuleTemplateTestToDomain)
            .ToList();

        var applyResult = Apply(template, request, editableValueTypes, tests);

        return applyResult.Map(ModuleMappings.MapModuleToDto);
    }

    private static Result<ModuleTemplate> Apply(
        ModuleTemplate template,
        UpdateModuleTemplateCommand request,
        IReadOnlyCollection<EntryValueDefinition> editableValueTypes,
        IReadOnlyCollection<ModuleTemplateTestDefinition> tests)
    {
        var renameResult = template.EditModule(request.Name);
        if (renameResult.IsFailure)
            return Result.Failure<ModuleTemplate>(renameResult.Error);

        var enabledResult = template.SetEnabled(request.Enabled);
        if (enabledResult.IsFailure)
            return Result.Failure<ModuleTemplate>(enabledResult.Error);

        var typeResult = template.ChangeType(request.Type);
        if (typeResult.IsFailure)
            return Result.Failure<ModuleTemplate>(typeResult.Error);

        var architectureResult = template.ReplaceArchitectureSupport(request.SupportedArchitectures);
        if (architectureResult.IsFailure)
            return Result.Failure<ModuleTemplate>(architectureResult.Error);

        var contentResult = template.ChangeContent(request.Content, editableValueTypes);
        if (contentResult.IsFailure)
            return Result.Failure<ModuleTemplate>(contentResult.Error);

        var testResult = template.ReconcileTests(tests);
        if (testResult.IsFailure)
            return Result.Failure<ModuleTemplate>(testResult.Error);

        return template;
    }
}