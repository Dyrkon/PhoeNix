using Microsoft.EntityFrameworkCore;
using PhoeNix.Contracts.Modules;
using PhoeNix.Application.Repositories;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Persistence.Repositories;

public sealed class ModuleTemplateReadRepository(
    ApplicationDbContext dbContext) : IModuleTemplateReadRepository
{
    public async Task<PagedResponse<ModuleTemplateListResponse>> GetPageAsync(
        ListModuleTemplatesRequest request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ModuleTemplates
            .AsNoTracking();

        query = ApplyFilters(query, request);
        query = ApplySorting(query, request);

        var totalItems = await query.CountAsync(cancellationToken);

        var templates = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = templates.Select(template => new ModuleTemplateListResponse(
            template.Id.Value,
            template.Name,
            template.Enabled,
            template.Type,
            template.SupportedArchitectures.ToList())).ToList();

        return new PagedResponse<ModuleTemplateListResponse>(items, totalItems);
    }

    private static IQueryable<ModuleTemplate> ApplyFilters(
        IQueryable<ModuleTemplate> query,
        ListModuleTemplatesRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(template =>
                template.Name.ToLower().Contains(search));
        }

        if (request.Enabled.HasValue)
            query = query.Where(template => template.Enabled == request.Enabled.Value);

        if (request.Type.HasValue)
            query = query.Where(template => template.Type == request.Type.Value);

        return query;
    }

    private static IQueryable<ModuleTemplate> ApplySorting(
        IQueryable<ModuleTemplate> query,
        ListModuleTemplatesRequest request)
    {
        return (request.SortField, request.SortDirection) switch
        {
            (ModuleTemplateSortField.Name, SortDirection.Ascending) => query.OrderBy(t => t.Name),
            (ModuleTemplateSortField.Name, SortDirection.Descending) => query.OrderByDescending(t => t.Name),

            (ModuleTemplateSortField.Type, SortDirection.Ascending) => query.OrderBy(t => t.Type),
            (ModuleTemplateSortField.Type, SortDirection.Descending) => query.OrderByDescending(t => t.Type),

            (ModuleTemplateSortField.Enabled, SortDirection.Ascending) => query.OrderBy(t => t.Enabled),
            (ModuleTemplateSortField.Enabled, SortDirection.Descending) => query.OrderByDescending(t => t.Enabled),

            _ => query.OrderBy(t => t.Name)
        };
    }
}
