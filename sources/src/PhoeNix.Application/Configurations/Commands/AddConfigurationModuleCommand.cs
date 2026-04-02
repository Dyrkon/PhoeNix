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

public sealed record AddConfigurationModuleCommand(
    ConfigurationId ConfigurationId,
    ModuleTemplateId ModuleTemplateId,
    bool Enabled) : ICommand<ModuleValueResponse>;

internal sealed class AddConfigurationModuleHandler(
    IConfigurationRepository configurationRepository,
    IModuleTemplateRepository moduleTemplateRepository)
    : ICommandHandler<AddConfigurationModuleCommand, ModuleValueResponse>
{
    public async Task<Result<ModuleValueResponse>> Handle(
        AddConfigurationModuleCommand request,
        CancellationToken cancellationToken)
    {
        var moduleTemplate = await moduleTemplateRepository.GetByIdAsync(
            request.ModuleTemplateId,
            cancellationToken);

        if (moduleTemplate is null)
            return Result.Failure<ModuleValueResponse>(ModuleErrors.NotFound(request.ModuleTemplateId));

        var configuration = await configurationRepository.GetByIdAsync(
            request.ConfigurationId,
            cancellationToken);

        if (configuration is null)
            return Result.Failure<ModuleValueResponse>(ConfigurationErrors.NotFound(request.ConfigurationId));

        return configuration.AddModule(moduleTemplate.Id, request.Enabled)
            .Bind(moduleValue => ModuleEntryFactory.CreateDefaultEntries(
                    moduleValue,
                    moduleTemplate.EditableValueTypes)
                .Bind(entries => moduleValue.ReplaceEntries(entries).Map(() => moduleValue)))
            .Map(ModuleMappings.MapModuleValueToDto);
    }
}