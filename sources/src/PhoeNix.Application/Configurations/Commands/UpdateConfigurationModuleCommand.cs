using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Application.Modules;
using PhoeNix.Application.Modules.Factories;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public sealed record UpdateConfigurationModuleCommand(
    ConfigurationId ConfigurationId,
    ModuleValueId ModuleValueId,
    bool Enabled,
    IReadOnlyList<ModuleEntryValueUpsertModel> Entries) : ICommand<ModuleValueResponse>;

internal sealed class UpdateConfigurationModuleHandler(
    IConfigurationRepository configurationRepository,
    IModuleTemplateRepository moduleTemplateRepository)
    : ICommandHandler<UpdateConfigurationModuleCommand, ModuleValueResponse>
{
    public async Task<Result<ModuleValueResponse>> Handle(
        UpdateConfigurationModuleCommand request,
        CancellationToken cancellationToken)
    {
        var configuration = await configurationRepository.GetByIdAsync(
            new ConfigurationId(request.ConfigurationId),
            cancellationToken);

        if (configuration is null)
            return Result.Failure<ModuleValueResponse>(ConfigurationErrors.NotFound(request.ConfigurationId));

        var moduleValue = configuration.Modules.FirstOrDefault(m => m.Id == new ModuleValueId(request.ModuleValueId));

        if (moduleValue is null)
            return Result.Failure<ModuleValueResponse>(
                new Error(
                    "Configurations.ModuleNotFound",
                    $"Module value '{request.ModuleValueId}' was not found in configuration '{configuration.Title}'."));

        var template = await moduleTemplateRepository.GetByIdAsync(moduleValue.ModuleTemplateId, cancellationToken);

        if (template is null)
            return Result.Failure<ModuleValueResponse>(
                new Error(
                    "Modules.TemplateNotFound",
                    $"Module template '{moduleValue.ModuleTemplateId.Value}' was not found."));

        return ModuleEntryFactory.CreateEntries(
                moduleValue,
                template.EditableValueTypes,
                request.Entries)
            .Bind(entries => configuration.UpdateModule(
                new ModuleValueId(request.ModuleValueId),
                request.Enabled,
                entries))
            .Map(ModuleMappings.MapModuleValueToDto);
    }
}