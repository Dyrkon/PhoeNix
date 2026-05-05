using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Queries;

public sealed record GetConfigurationByIdQuery(ConfigurationId ConfigurationId) : IQuery<ConfigurationResponse>;

internal sealed class GetConfigurationByIdHandler(
    IConfigurationRepository configurationRepository,
    IModuleTemplateRepository moduleTemplateRepository,
    ICurrentUserAccessor currentUserAccessor)
    : IQueryHandler<GetConfigurationByIdQuery, ConfigurationResponse>
{
    public Task<Result<ConfigurationResponse>> Handle(
        GetConfigurationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Task.FromResult(Result.Failure<ConfigurationResponse>(userIdResult.Error));

        return configurationRepository
            .GetByIdAsync(request.ConfigurationId, cancellationToken)
            .EnsureNotNull(ConfigurationErrors.NotFound(request.ConfigurationId))
            .Ensure(c => c.OwnerId == userIdResult.Value, ConfigurationErrors.NotFound(request.ConfigurationId))
            .Bind(async configuration =>
            {
                var moduleTemplateIds = configuration.Modules
                    .Select(module => module.ModuleTemplateId)
                    .Concat(configuration.SystemSpecifications.SelectMany(system => system.Modules.Select(module => module.ModuleTemplateId)))
                    .Distinct()
                    .ToList();

                var moduleTemplates = await moduleTemplateRepository.GetByIdsAsync(moduleTemplateIds, userIdResult.Value, cancellationToken);
                var templatesById = moduleTemplates.ToDictionary(template => template.Id);

                return Result.Success(ConfigurationMappings.MapConfigurationToDto(configuration, templatesById));
            });
    }
}