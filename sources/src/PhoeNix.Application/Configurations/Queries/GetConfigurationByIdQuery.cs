using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Queries;

public sealed record GetConfigurationByIdQuery(ConfigurationId ConfigurationId)
    : IQuery<ConfigurationWithRevisionsResponse>;

internal sealed class GetConfigurationByIdHandler(
    IConfigurationRepository configurationRepository,
    IModuleTemplateRepository moduleTemplateRepository)
    : IQueryHandler<GetConfigurationByIdQuery, ConfigurationWithRevisionsResponse>
{
    public Task<Result<ConfigurationWithRevisionsResponse>> Handle(
        GetConfigurationByIdQuery request,
        CancellationToken cancellationToken)
    {
        return configurationRepository
            .GetByIdAsync(request.ConfigurationId, cancellationToken)
            .EnsureNotNull(ConfigurationErrors.NotFound(request.ConfigurationId))
            .Bind(async configuration =>
            {
                var moduleTemplateIds = configuration.Modules
                    .Select(module => module.ModuleTemplateId)
                    .Concat(configuration.SystemSpecifications.SelectMany(system =>
                        system.Modules.Select(module => module.ModuleTemplateId)))
                    .Distinct()
                    .ToList();

                var moduleTemplates =
                    await moduleTemplateRepository.GetByIdsAsync(moduleTemplateIds, cancellationToken);
                var templatesById = moduleTemplates.ToDictionary(template => template.Id);

                return Result.Success(
                    ConfigurationMappings.MapConfigurationWithRevisionsToDto(configuration, templatesById));
            });
    }
}