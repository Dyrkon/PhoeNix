using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Configurations;

namespace PhoeNix.Persistence.Extensions;

public static class ConfigurationRepositoryExtensions
{
    public static IQueryable<Configuration> AddIncludeStatements(this IQueryable<Configuration> query)
    {
        return query
            .Include(c => c.Modules)
            .ThenInclude(m => m.EditableValues)
            .Include(c => c.Inputs)
            .ThenInclude(i => i.Followers)
            .Include(c => c.SystemSpecifications)
            .ThenInclude(s => s.Modules)
            .ThenInclude(s => s.EditableValues)
            .Include(c => c.Revisions);
    }
}