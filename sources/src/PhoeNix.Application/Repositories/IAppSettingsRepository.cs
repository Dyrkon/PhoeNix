using PhoeNix.Domain.Entities.AppSettings;

namespace PhoeNix.Application.Repositories;

public interface IAppSettingsRepository
{
    Task<AppSettings?> GetAsync(CancellationToken cancellationToken = default);
}
