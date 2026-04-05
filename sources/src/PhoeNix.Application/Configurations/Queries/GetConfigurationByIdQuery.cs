using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Configurations;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Queries;

public sealed record GetConfigurationByIdQuery(ConfigurationId ConfigurationId) : IQuery<ConfigurationResponse>;

internal sealed class GetConfigurationByIdHandler(
    IConfigurationRepository configurationRepository,
    IModuleTemplateRepository moduleTemplateRepository)
    : IQueryHandler<GetConfigurationByIdQuery, ConfigurationResponse>
{
    public async Task<Result<ConfigurationResponse>> Handle(
        GetConfigurationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var configuration = await configurationRepository.GetByIdAsync(
            request.ConfigurationId,
            cancellationToken);

        if (configuration is null)
            return Result.Failure<ConfigurationResponse>(
                ConfigurationErrors.NotFound(request.ConfigurationId));

        var moduleTemplateIds = configuration.Modules
            .Select(module => module.ModuleTemplateId)
            .Concat(configuration.SystemSpecifications.SelectMany(system => system.Modules.Select(module => module.ModuleTemplateId)))
            .Distinct()
            .ToList();

        var moduleTemplates = await moduleTemplateRepository.GetByIdsAsync(
            moduleTemplateIds,
            cancellationToken);

        var templatesById = moduleTemplates.ToDictionary(template => template.Id);

        return Result.Success(
            ConfigurationMappings.MapConfigurationToDto(configuration, templatesById));
    }
}