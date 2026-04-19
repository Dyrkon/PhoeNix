using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Commands;

public sealed record UpdateConfigurationCommand(
    ConfigurationId ConfigurationId,
    string Title,
    string Description) : ICommand<ConfigurationResponse>;

internal sealed class UpdateConfigurationHandler(
    IConfigurationRepository configurationRepository,
    IModuleTemplateRepository moduleTemplateRepository)
    : ICommandHandler<UpdateConfigurationCommand, ConfigurationResponse>
{
    public Task<Result<ConfigurationResponse>> Handle(
        UpdateConfigurationCommand request,
        CancellationToken cancellationToken)
    {
        return configurationRepository
            .GetByIdAsync(request.ConfigurationId, cancellationToken)
            .EnsureNotNull(ConfigurationErrors.NotFound(request.ConfigurationId))
            .Tap(configuration => configuration.EditConfiguration(request.Title, request.Description))
            .Bind(async configuration =>
            {
                var moduleTemplateIds = configuration.Modules
                    .Select(x => x.ModuleTemplateId)
                    .Distinct()
                    .ToList();

                var moduleTemplates = await moduleTemplateRepository.GetByIdsAsync(moduleTemplateIds, cancellationToken);
                var templatesById = moduleTemplates.ToDictionary(x => x.Id);

                return Result.Success(ConfigurationMappings.MapConfigurationToDto(configuration, templatesById));
            });
    }
}