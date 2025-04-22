using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Configurations;

namespace PhoeNix.Persistence.Extensions;

public static class ConfigurationRepositoryExtensions
{
    public static IQueryable<Configuration> AddIncludeStatements(this IQueryable<Configuration> query) =>
        query
            .Include(c => c.Modules)
            .ThenInclude(m => m.Module)
            .Include(c => c.Homes)
            .ThenInclude(h => h.Home)
            .Include(c => c.Inputs)
            .ThenInclude(i => i.Input)
            .Include(c => c.Systems)
            .ThenInclude(s => s.System);
}
