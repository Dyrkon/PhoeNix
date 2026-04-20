using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Mappings;
using PhoeNix.Contracts.Configurations;
using PhoeNix.Application.Repositories;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Persistence.Extensions;

namespace PhoeNix.Persistence.Repositories;

public sealed class ConfigurationReadRepository(
    ApplicationDbContext dbContext,
    IConfigurationRepository configurationRepository,
    IModuleTemplateRepository moduleTemplateRepository) : IConfigurationReadRepository
{
    public async Task<PagedResponse<ConfigurationListResponse>> GetPageAsync(
        ListConfigurationsRequest request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Configurations
            .AsNoTracking();

        query = ApplyFilters(query, request);
        query = ApplySorting(query, request);

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(configuration => new ConfigurationListResponse(
                configuration.Id.Value,
                configuration.Title,
                configuration.Description))
            .ToListAsync(cancellationToken);

        return new PagedResponse<ConfigurationListResponse>(items, totalItems);
    }

    public async Task<ConfigurationResponse?> GetByIdAsync(
        ConfigurationId configurationId,
        CancellationToken cancellationToken)
    {
        var configuration = await configurationRepository.GetByIdAsync(
            configurationId,
            cancellationToken);

        if (configuration is null)
            return null;

        var moduleTemplateIds = configuration.Modules
            .Select(module => module.ModuleTemplateId)
            .Distinct()
            .ToList();

        var moduleTemplates = await moduleTemplateRepository.GetByIdsAsync(
            moduleTemplateIds,
            cancellationToken);

        var templatesById = moduleTemplates.ToDictionary(template => template.Id);

        return ConfigurationMappings.MapConfigurationToDto(configuration, templatesById);
    }

    public async Task<IReadOnlyList<Configuration>> GetByIdsAsync(IReadOnlyCollection<ConfigurationId> ids,
        CancellationToken token)
    {
        if (ids.Count == 0)
            return [];

        return await dbContext.Configurations
            .AddIncludeStatements()
            .Where(m => ids.Contains(m.Id))
            .OrderBy(m => m.Title)
            .ToListAsync(token);
    }

    private static IQueryable<Configuration> ApplyFilters(
        IQueryable<Configuration> query,
        ListConfigurationsRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();

            query = query.Where(configuration =>
                configuration.Title.ToLower().Contains(search) ||
                configuration.Description.ToLower().Contains(search));
        }

        return query;
    }

    private static IQueryable<Configuration> ApplySorting(
        IQueryable<Configuration> query,
        ListConfigurationsRequest request)
    {
        return (request.SortField, request.SortDirection) switch
        {
            (ConfigurationSortField.Title, SortDirection.Ascending) => query.OrderBy(configuration =>
                configuration.Title),
            (ConfigurationSortField.Title, SortDirection.Descending) => query.OrderByDescending(configuration =>
                configuration.Title),

            (ConfigurationSortField.Description, SortDirection.Ascending) => query.OrderBy(configuration =>
                configuration.Description),
            (ConfigurationSortField.Description, SortDirection.Descending) => query.OrderByDescending(configuration =>
                configuration.Description),

            _ => query.OrderBy(configuration => configuration.Title)
        };
    }
}