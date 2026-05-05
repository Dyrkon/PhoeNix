using PhoeNix.Common.Models;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Application.Repositories;

public interface IConfigurationReadRepository
{
    Task<PagedResponse<ConfigurationListResponse>> GetPageAsync(
        ListConfigurationsRequest request,
        UserId ownerId,
        CancellationToken cancellationToken);

    Task<ConfigurationResponse?> GetByIdAsync(
        ConfigurationId configurationId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Configuration>> GetByIdsAsync(
        IReadOnlyCollection<ConfigurationId> ids,
        CancellationToken token);
}