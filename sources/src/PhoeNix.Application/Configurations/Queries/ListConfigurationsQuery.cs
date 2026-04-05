using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Configurations;
using PhoeNix.Application.Repositories;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Configurations.Queries;

public sealed record ListConfigurationsQuery(ListConfigurationsRequest Request)
    : IQuery<PagedResponse<ConfigurationListResponse>>;

internal sealed class ListConfigurationsQueryHandler(
    IConfigurationReadRepository configurationReadRepository)
    : IQueryHandler<ListConfigurationsQuery, PagedResponse<ConfigurationListResponse>>
{
    public async Task<Result<PagedResponse<ConfigurationListResponse>>> Handle(
        ListConfigurationsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Request.Page <= 0)
            return Result.Failure<PagedResponse<ConfigurationListResponse>>(new Error(
                "Configurations.InvalidPage",
                "Page must be greater than zero."));

        if (request.Request.PageSize <= 0)
            return Result.Failure<PagedResponse<ConfigurationListResponse>>(new Error(
                "Configurations.InvalidPageSize",
                "Page size must be greater than zero."));

        var response = await configurationReadRepository.GetPageAsync(request.Request, cancellationToken);

        return Result.Success(response);
    }
}