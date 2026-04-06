using PhoeNix.Application.Models.Configurations;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Entities.Configurations;

namespace PhoeNix.Application.Repositories;

public interface IConfigurationReadRepository
{
    Task<PagedResponse<ConfigurationListResponse>> GetPageAsync(
        ListConfigurationsRequest request,
        CancellationToken cancellationToken);

    Task<ConfigurationResponse?> GetByIdAsync(
        ConfigurationId configurationId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Configuration>> GetByIdsAsync(
        IReadOnlyCollection<ConfigurationId> ids,
        CancellationToken token);
}