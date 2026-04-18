using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.AppSettings;

namespace PhoeNix.Persistence.Repositories;

public sealed class AppSettingsRepository(ApplicationDbContext dbContext) : IAppSettingsRepository
{
    public Task<AppSettings?> GetAsync(CancellationToken cancellationToken = default)
        => dbContext.AppSettings.FirstOrDefaultAsync(cancellationToken);
}
