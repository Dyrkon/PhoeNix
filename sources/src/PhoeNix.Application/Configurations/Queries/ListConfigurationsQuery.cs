using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Repositories;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Queries;

public sealed record ListConfigurationsQuery(ListConfigurationsRequest Request)
    : IQuery<PagedResponse<ConfigurationListResponse>>;

internal sealed class ListConfigurationsQueryHandler(
    IConfigurationReadRepository configurationReadRepository)
    : IQueryHandler<ListConfigurationsQuery, PagedResponse<ConfigurationListResponse>>
{
    public Task<Result<PagedResponse<ConfigurationListResponse>>> Handle(
        ListConfigurationsQuery request,
        CancellationToken cancellationToken)
    {
        return Result.Success(request.Request)
            .Ensure(r => r.Page > 0, new Error("Configurations.InvalidPage", "Page must be greater than zero."))
            .Ensure(r => r.PageSize > 0, new Error("Configurations.InvalidPageSize", "Page size must be greater than zero."))
            .Map(r => configurationReadRepository.GetPageAsync(r, cancellationToken));
    }
}