using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Configurations;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Queries;

public sealed record GetConfigurationByIdQuery(ConfigurationId ConfigurationId) : IQuery<ConfigurationResponse>;

internal sealed class GetConfigurationByIdHandler(
    IConfigurationRepository configurationRepository)
    : IQueryHandler<GetConfigurationByIdQuery, ConfigurationResponse>
{
    public async Task<Result<ConfigurationResponse>> Handle(
        GetConfigurationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var configuration = await configurationRepository.GetByIdAsync(
            request.ConfigurationId,
            cancellationToken);

        return configuration
            .EnsureNotNull(ConfigurationErrors.NotFound(request.ConfigurationId))
            .Map(ConfigurationMappings.MapConfigurationToDto);
    }
}