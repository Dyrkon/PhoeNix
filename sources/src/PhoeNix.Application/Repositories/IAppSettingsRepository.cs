using PhoeNix.Domain.Entities.AppSettings;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Application.Repositories;

public interface IAppSettingsRepository
{
    Task<AppSettings?> GetAsync(UserId ownerId, CancellationToken cancellationToken = default);
    Task<AppSettings?> GetFirstAsync(CancellationToken cancellationToken = default);
}
