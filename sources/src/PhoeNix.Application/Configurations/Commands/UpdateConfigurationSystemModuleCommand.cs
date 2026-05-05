using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Application.Modules.Factories;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public sealed record UpdateConfigurationSystemModuleCommand(
    ConfigurationId ConfigurationId,
    SystemId SystemId,
    ModuleValueId ModuleValueId,
    bool Enabled,
    IReadOnlyList<ModuleEntryValueUpsertModel> Entries) : ICommand<ModuleValueResponse>;

internal sealed class UpdateConfigurationSystemModuleHandler(
    IConfigurationRepository configurationRepository,
    IModuleTemplateRepository moduleTemplateRepository,
    ICurrentUserAccessor currentUserAccessor)
    : ICommandHandler<UpdateConfigurationSystemModuleCommand, ModuleValueResponse>
{
    public async Task<Result<ModuleValueResponse>> Handle(
        UpdateConfigurationSystemModuleCommand request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Result.Failure<ModuleValueResponse>(userIdResult.Error);

        var configuration = await configurationRepository.GetByIdAsync(
            new ConfigurationId(request.ConfigurationId),
            cancellationToken);

        if (configuration is null)
            return Result.Failure<ModuleValueResponse>(ConfigurationErrors.NotFound(request.ConfigurationId));

        if (configuration.OwnerId != userIdResult.Value)
            return Result.Failure<ModuleValueResponse>(ConfigurationErrors.NotFound(request.ConfigurationId));

        var system = configuration.SystemSpecifications.FirstOrDefault(s => s.Id == new SystemId(request.SystemId));

        if (system is null)
            return Result.Failure<ModuleValueResponse>(
                new Error("Configurations.SystemNotFound",
                    $"System '{request.SystemId}' was not found in configuration '{configuration.Title}'."));

        var moduleValue = system.Modules.FirstOrDefault(m => m.Id == new ModuleValueId(request.ModuleValueId));

        if (moduleValue is null)
            return Result.Failure<ModuleValueResponse>(
                new Error("Configurations.SystemModuleNotFound",
                    $"Module value '{request.ModuleValueId}' was not found in system '{request.SystemId}'."));

        var template = await moduleTemplateRepository.GetByIdAsync(moduleValue.ModuleTemplateId, cancellationToken);

        if (template is null)
            return Result.Failure<ModuleValueResponse>(
                new Error("Modules.TemplateNotFound",
                    $"Module template '{moduleValue.ModuleTemplateId.Value}' was not found."));

        return ModuleEntryFactory.CreateEntries(moduleValue, template.EditableValueTypes, request.Entries)
            .Bind(entries => configuration.UpdateSystemModule(
                new SystemId(request.SystemId),
                new ModuleValueId(request.ModuleValueId),
                request.Enabled,
                entries))
            .Map(ModuleMappings.MapModuleValueToDto);
    }
}