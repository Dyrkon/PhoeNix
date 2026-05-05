using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Application.Modules;
using PhoeNix.Application.Modules.Factories;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public sealed record AddConfigurationSystemModuleCommand(
    ConfigurationId ConfigurationId,
    SystemId SystemId,
    ModuleTemplateId ModuleTemplateId,
    bool Enabled) : ICommand<ModuleValueResponse>;

internal sealed class AddConfigurationSystemModuleHandler(
    IConfigurationRepository configurationRepository,
    IModuleTemplateRepository moduleTemplateRepository,
    ICurrentUserAccessor currentUserAccessor)
    : ICommandHandler<AddConfigurationSystemModuleCommand, ModuleValueResponse>
{
    public async Task<Result<ModuleValueResponse>> Handle(
        AddConfigurationSystemModuleCommand request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<ModuleValueResponse>(userIdResult.Error);

        var template = await moduleTemplateRepository.GetByIdAsync(
            request.ModuleTemplateId,
            cancellationToken);

        if (template is null)
            return Result.Failure<ModuleValueResponse>(ModuleErrors.NotFound(request.ModuleTemplateId));

        var configuration = await configurationRepository.GetByIdAsync(
            request.ConfigurationId,
            cancellationToken);

        if (configuration is null)
            return Result.Failure<ModuleValueResponse>(ConfigurationErrors.NotFound(request.ConfigurationId));

        if (configuration.OwnerId != userIdResult.Value)
            return Result.Failure<ModuleValueResponse>(ConfigurationErrors.NotFound(request.ConfigurationId));

        foreach (var required in template.RequiredInputs)
            configuration.AddInput(required.Source, required.Name);

        return configuration.AddSystemModule(
                new SystemId(request.SystemId),
                template.Id,
                template.SupportedArchitectures.ToList(),
                request.Enabled)
            .Bind(moduleValue => ModuleEntryFactory.CreateDefaultEntries(
                    moduleValue,
                    template.EditableValueTypes)
                .Bind(entries => moduleValue.ReplaceEntries(entries).Map(() => moduleValue)))
            .Map(ModuleMappings.MapModuleValueToDto);
    }
}