using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Configurations;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Queries;

public sealed record GetConfigurationsQuery() : IQuery<IReadOnlyList<ConfigurationListResponse>>;

internal sealed class GetConfigurationsHandler(
    IConfigurationRepository configurationRepository)
    : IQueryHandler<GetConfigurationsQuery, IReadOnlyList<ConfigurationListResponse>>
{
    public async Task<Result<IReadOnlyList<ConfigurationListResponse>>> Handle(
        GetConfigurationsQuery request,
        CancellationToken cancellationToken)
    {
        var configurations = await configurationRepository.GetAllAsync(cancellationToken);

        return Result.Success<IReadOnlyList<ConfigurationListResponse>>(
            configurations.Select(ConfigurationMappings.MapConfigurationToListDto).ToList());
    }
}