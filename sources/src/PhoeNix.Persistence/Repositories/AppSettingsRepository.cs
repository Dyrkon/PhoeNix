using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.AppSettings;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Persistence.Repositories;

public sealed class AppSettingsRepository(ApplicationDbContext dbContext) : IAppSettingsRepository
{
    public Task<AppSettings?> GetAsync(UserId ownerId, CancellationToken cancellationToken = default)
        => dbContext.AppSettings.FirstOrDefaultAsync(s => s.OwnerId == ownerId, cancellationToken);

    public Task<AppSettings?> GetFirstAsync(CancellationToken cancellationToken = default)
        => dbContext.AppSettings.FirstOrDefaultAsync(cancellationToken);
}
