using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Configurations;

namespace PhoeNix.Persistence.Extensions;

public static class ConfigurationRepositoryExtensions
{
    public static IQueryable<Configuration> AddIncludeStatements(this IQueryable<Configuration> query)
    {
        return query
            .Include(c => c.Modules)
            .ThenInclude(m => m.Module)
            .ThenInclude(m => m.EditableValues)
            .Include(c => c.Modules)
            .ThenInclude(m => m.Module)
            .ThenInclude(m => m.Tests)
            .ThenInclude(m => m.Test)
            .Include(c => c.Inputs)
            .ThenInclude(i => i.Followers)
            .Include(c => c.Systems)
            .ThenInclude(s => s.System)
            .ThenInclude(s => s.Modules)
            .ThenInclude(sm => sm.Module)
            .ThenInclude(m => m.EditableValues)
            .Include(c => c.Systems)
            .ThenInclude(s => s.System)
            .ThenInclude(s => s.Modules)
            .ThenInclude(sm => sm.Module)
            .ThenInclude(s => s.Tests)
            .ThenInclude(s => s.Test);
    }
}